DROP TYPE IF EXISTS public.tabledefs CASCADE;
CREATE TYPE public.tabledefs AS ENUM ('PKEY_INTERNAL','PKEY_EXTERNAL','FKEYS_INTERNAL', 'FKEYS_EXTERNAL', 'FKEYS_COMMENTED', 'FKEYS_NONE', 'INCLUDE_TRIGGERS', 'NO_TRIGGERS');

-- DROP FUNCTION public.pg_get_coldef(text,text,text,boolean);
CREATE OR REPLACE FUNCTION public.pg_get_coldef(
  in_schema text,
  in_table text,
  in_column text,
  oldway boolean default False
)
RETURNS text
LANGUAGE plpgsql VOLATILE
AS
$$
DECLARE
coldef text;
BEGIN
  IF oldway THEN 
    SELECT pg_catalog.format_type(a.atttypid, a.atttypmod) INTO coldef FROM pg_namespace n, pg_class c, pg_attribute a, pg_type t 
    WHERE n.nspname = in_schema AND n.oid = c.relnamespace AND c.relname = in_table AND a.attname = in_column and a.attnum > 0 AND a.attrelid = c.oid AND a.atttypid = t.oid ORDER BY a.attnum;
  ELSE
    -- a.attrelid::regclass::text, a.attname
    SELECT CASE WHEN a.atttypid = ANY ('{int,int8,int2}'::regtype[]) AND EXISTS (SELECT FROM pg_attrdef ad WHERE ad.adrelid = a.attrelid AND ad.adnum   = a.attnum AND 
	  pg_get_expr(ad.adbin, ad.adrelid) = 'nextval(''' || (pg_get_serial_sequence (a.attrelid::regclass::text, a.attname))::regclass || '''::regclass)') THEN CASE a.atttypid 
	  WHEN 'int'::regtype  THEN 'serial' WHEN 'int8'::regtype THEN 'bigserial' WHEN 'int2'::regtype THEN 'smallserial' END ELSE format_type(a.atttypid, a.atttypmod) END AS data_type  
	  INTO coldef FROM pg_namespace n, pg_class c, pg_attribute a, pg_type t 
	  WHERE n.nspname = in_schema AND n.oid = c.relnamespace AND c.relname = in_table AND a.attname = in_column and a.attnum > 0 AND a.attrelid = c.oid AND a.atttypid = t.oid ORDER BY a.attnum;
  END IF;
  RETURN coldef;
END;
$$;

-- SELECT * FROM public.pg_get_tabledef('sample', 'address', false);
DROP FUNCTION IF EXISTS public.pg_get_tabledef(character varying,character varying,boolean,tabledefs[]);
CREATE OR REPLACE FUNCTION public.pg_get_tabledef(
  in_schema varchar,
  in_table varchar,
  _verbose boolean,
  VARIADIC arr public.tabledefs[] DEFAULT '{}':: public.tabledefs[]
)
RETURNS text
LANGUAGE plpgsql VOLATILE
AS
$$
 /* ********************************************************************************
COPYRIGHT NOTICE FOLLOWS.  DO NOT REMOVE
Copyright (c) 2021-2023 SQLEXEC LLC

Permission to use, copy, modify, and distribute this software and its documentation 
for any purpose, without fee, and without a written agreement is hereby granted, 
provided that the above copyright notice and this paragraph and the following two paragraphs appear in all copies.

IN NO EVENT SHALL SQLEXEC LLC BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT,INDIRECT SPECIAL, 
INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE 
OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF SQLEXEC LLC HAS BEEN ADVISED OF THE 
POSSIBILITY OF SUCH DAMAGE.

SQLEXEC LLC SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. 
THE SOFTWARE PROVIDED HEREUNDER IS ON AN "AS IS" BASIS, AND SQLEXEC LLC HAS 
NO OBLIGATIONS TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

************************************************************************************ */

-- History:
-- Date	     Description
-- ==========   ======================================================================  
-- 2021-03-20   Original coding using some snippets from 
--              https://stackoverflow.com/questions/2593803/how-to-generate-the-create-table-sql-statement-for-an-existing-table-in-postgr
-- 2021-03-21   Added partitioned table support, i.e., PARTITION BY clause.
-- 2021-03-21   Added WITH clause logic where storage parameters for tables are set.
-- 2021-03-22   Added tablespace logic for tables and indexes.
-- 2021-03-24   Added inheritance-based partitioning support for PG 9.6 and lower.
-- 2022-09-12   Fixed Issue#1: Added fix for PostGIS columns where we do not presume the schema, leave without schema to imply public schema
-- 2022-09-19   Fixed Issue#2: Do not add CREATE INDEX statements if the indexes are defined within the Table definition as ADD CONSTRAINT.
-- 2022-12-03   Fixed: Handle NULL condition for ENUMs
-- 2022-12-07   Fixed: not setting tablespace correctly for user defined tablespaces
-- 2023-04-12   Fixed Issue#6: Handle array types: int, bigint, varchar, even varchars with precisions.
-- 2023-04-13   Fixed Issue#7: Incomplete fixing of issue#6
-- 2023-04-21   Fixed Issue#8: previously returns actual sequence info (aka \d) instead of serial/bigserial def.
-- 2023-04-21   Fixed Issue#10: Consolidated comments into one place under function prototype heading.
-- 2023-05-17   Fixed Issue#13: do not specify FKEY for partitions. It is done on the parent and implied on the partitions, else you get "fkey already exists" error
-- 2023-05-20   Fixed syntax error, missing THEN keyword
-- 2023-05-20   Fixed Issue#11: Handle parent of table being in another schema
-- 2023-07-24   Fixed Issue#14: If multiple triggers are defined on a table, show them all not just the first one.
-- 2023-08-03   Fixed Issue#15: use utd_schema with USER-DEFINED data types, not defaulting to table schema.
-- 2023-08-03   Fixed Issue#16: Make it optional to define the PKEY as external instead of internal.
-- 2023-08-24   Fixed Issue#17: Handle case-sensitive tables.
-- 2023-08-26   Fixed Issue#17: Had to remove quote_ident when identifying case sensitive tables
-- 2023-08-28   Fixed Issue#19: Identified in pull request#18: double-quote reserved keywords
-- 2023-xx-xx   Future enhancemart start for allowing external PK def

  DECLARE
    v_qualified text := '';
    v_table_ddl text;
    v_table_oid int;
    v_colrec record;
    v_constraintrec record;
    v_trigrec       record;
    v_indexrec record;
    v_primary boolean := False;
    v_constraint_name text;
    v_constraint_def  text;
    v_pkey_def        text := '';
    v_fkey_defs text;
    v_trigger text := '';
    v_partition_key text := '';
    v_partbound text;
    v_parent text;
    v_parent_schema text;
    v_persist text;
    v_temp  text := ''; 
    v_relopts text;
    v_tablespace text;
    v_pgversion int;
    bSerial boolean;
    bPartition boolean;
    bInheritance boolean;
    bRelispartition boolean;
    constraintarr text[] := '{}';
    constraintelement text;
    bSkip boolean;
	  bVerbose boolean := False;
	  v_cnt1   integer;
	  v_cnt2   integer;

    -- assume defaults for ENUMs at the getgo	
  	pkcnt            int := 0;
  	fkcnt            int := 0;
	  trigcnt          int := 0;
    pktype           tabledefs := 'PKEY_INTERNAL';
    fktype           tabledefs := 'FKEYS_INTERNAL';
    trigtype         tabledefs := 'NO_TRIGGERS';
    arglen           integer;
  	vargs            text;
	  avarg            tabledefs;

    -- exception variables
    v_ret            text;
    v_diag1          text;
    v_diag2          text;
    v_diag3          text;
    v_diag4          text;
    v_diag5          text;
    v_diag6          text;
	
  BEGIN
    SET client_min_messages = 'notice';
    IF _verbose THEN bVerbose = True; END IF;
    
    -- v17 fix: handle case-sensitive  
    -- v_qualified = in_schema || '.' || in_table;
	
    arglen := array_length($4, 1);
    IF arglen IS NULL THEN
        -- nothing to do, so assume defaults
        NULL;
    ELSE
        -- loop thru args
        -- IF 'NO_TRIGGERS' = ANY ($4)
        -- select array_to_string($4, ',', '***') INTO vargs;
        IF bVerbose THEN RAISE NOTICE 'arguments=%', $4; END IF;
        FOREACH avarg IN ARRAY $4 LOOP
            IF bVerbose THEN RAISE INFO 'arg=%', avarg; END IF;
            IF avarg = 'FKEYS_INTERNAL' OR avarg = 'FKEYS_EXTERNAL' OR avarg = 'FKEYS_COMMENTED' THEN
                fkcnt = fkcnt + 1;
                fktype = avarg;
            ELSEIF avarg = 'INCLUDE_TRIGGERS' OR avarg = 'NO_TRIGGERS' THEN
                trigcnt = trigcnt + 1;
                trigtype = avarg;
            ELSEIF avarg = 'PKEY_EXTERNAL' THEN
                pkcnt = pkcnt + 1;
                pktype = avarg;				                
            END IF;
        END LOOP;
        IF fkcnt > 1 THEN 
  	        RAISE WARNING 'Only one foreign key option can be provided. You provided %', fkcnt;
	          RETURN '';
        ELSEIF trigcnt > 1 THEN 
            RAISE WARNING 'Only one trigger option can be provided. You provided %', trigcnt;
            RETURN '';
        ELSEIF pkcnt > 1 THEN 
            RAISE WARNING 'Only one pkey option can be provided. You provided %', pkcnt;
            RETURN '';			
        END IF;		   		   
    END IF;

    SELECT c.oid, (select setting from pg_settings where name = 'server_version_num') INTO v_table_oid, v_pgversion FROM pg_catalog.pg_class c LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
    WHERE c.relkind in ('r','p') AND c.relname = in_table AND n.nspname = in_schema;
    	
    -- throw an error if table was not found
    IF (v_table_oid IS NULL) THEN
      RAISE EXCEPTION 'table does not exist';
    END IF;

    -- get user-defined tablespaces if applicable
    SELECT tablespace INTO v_temp FROM pg_tables WHERE schemaname = in_schema and tablename = in_table and tablespace IS NOT NULL;
    IF v_temp IS NULL THEN
      v_tablespace := 'TABLESPACE pg_default';
    ELSE
      v_tablespace := 'TABLESPACE ' || v_temp;
    END IF;
    
    -- also see if there are any SET commands for this table, ie, autovacuum_enabled=off, fillfactor=70
    WITH relopts AS (SELECT unnest(c.reloptions) relopts FROM pg_class c, pg_namespace n WHERE n.nspname = in_schema and n.oid = c.relnamespace and c.relname = in_table) 
    SELECT string_agg(r.relopts, ', ') as relopts INTO v_temp from relopts r;
    IF v_temp IS NULL THEN
      v_relopts := '';
    ELSE
      v_relopts := ' WITH (' || v_temp || ')';
    END IF;
    
    -- -----------------------------------------------------------------------------------
    -- Create table defs for partitions/children using inheritance or declarative methods.
    -- inheritance: pg_class.relkind = 'r'   pg_class.relispartition=false   pg_class.relpartbound is NULL
    -- declarative: pg_class.relkind = 'r'   pg_class.relispartition=true    pg_class.relpartbound is NOT NULL
    -- -----------------------------------------------------------------------------------
    v_partbound := '';
    bPartition := False;
    bInheritance := False;
    IF v_pgversion < 100000 THEN
      -- Issue#11: handle parent schema
      SELECT c2.relname parent, c2.relnamespace::regnamespace INTO v_parent, v_parent_schema from pg_class c1, pg_namespace n, pg_inherits i, pg_class c2
      WHERE n.nspname = in_schema and n.oid = c1.relnamespace and c1.relname = in_table and c1.oid = i.inhrelid and i.inhparent = c2.oid and c1.relkind = 'r';      
      IF (v_parent IS NOT NULL) THEN
        bPartition   := True;
        bInheritance := True;
      END IF;
    ELSE
      -- Issue#11: handle parent schema
      SELECT c2.relname parent, c1.relispartition, pg_get_expr(c1.relpartbound, c1.oid, true), c2.relnamespace::regnamespace INTO v_parent, bRelispartition, v_partbound, v_parent_schema from pg_class c1, pg_namespace n, pg_inherits i, pg_class c2
      WHERE n.nspname = in_schema and n.oid = c1.relnamespace and c1.relname = in_table and c1.oid = i.inhrelid and i.inhparent = c2.oid and c1.relkind = 'r';
      IF (v_parent IS NOT NULL) THEN
        bPartition   := True;
        IF bRelispartition THEN
          bInheritance := False;
        ELSE
          bInheritance := True;
        END IF;
      END IF;
    END IF;
    IF bPartition THEN
      --Issue#17 fix for case-sensitive tables
		  -- SELECT count(*) INTO v_cnt1 FROM information_schema.tables t WHERE EXISTS (SELECT REGEXP_MATCHES(s.table_name, '([A-Z]+)','g') FROM information_schema.tables s 
		  -- WHERE t.table_schema=s.table_schema AND t.table_name=s.table_name AND t.table_schema = quote_ident(in_schema) AND t.table_name = quote_ident(in_table) AND t.table_type = 'BASE TABLE');      
		  SELECT count(*) INTO v_cnt1 FROM information_schema.tables t WHERE EXISTS (SELECT REGEXP_MATCHES(s.table_name, '([A-Z]+)','g') FROM information_schema.tables s 
		  WHERE t.table_schema=s.table_schema AND t.table_name=s.table_name AND t.table_schema = in_schema AND t.table_name = in_table AND t.table_type = 'BASE TABLE');      		  
		  
      --Issue#19 put double-quotes around SQL keyword column names
      SELECT COUNT(*) INTO v_cnt2 FROM pg_get_keywords() WHERE word = v_colrec.column_name AND catcode = 'R';
		  
      IF bInheritance THEN
        -- inheritance-based
        IF v_cnt1 > 0 OR v_cnt2 > 0 THEN
          v_table_ddl := 'CREATE TABLE ' || in_schema || '."' || in_table || '"( '|| E'\n';        
        ELSE
          v_table_ddl := 'CREATE TABLE ' || in_schema || '.' || in_table || '( '|| E'\n';                
        END IF;

        -- Jump to constraints section to add the check constraints
      ELSE
        -- declarative-based
        IF v_relopts <> '' THEN
          IF v_cnt1 > 0 OR v_cnt2 > 0 THEN
            v_table_ddl := 'CREATE TABLE ' || in_schema || '."' || in_table || '" PARTITION OF ' || in_schema || '.' || v_parent || ' ' || v_partbound || v_relopts || ' ' || v_tablespace || '; ' || E'\n';
				  ELSE
				    v_table_ddl := 'CREATE TABLE ' || in_schema || '.' || in_table || ' PARTITION OF ' || in_schema || '.' || v_parent || ' ' || v_partbound || v_relopts || ' ' || v_tablespace || '; ' || E'\n';
				  END IF;
        ELSE
          IF v_cnt1 > 0 OR v_cnt2 > 0 THEN
            v_table_ddl := 'CREATE TABLE ' || in_schema || '."' || in_table || '" PARTITION OF ' || in_schema || '.' || v_parent || ' ' || v_partbound || ' ' || v_tablespace || '; ' || E'\n';
				  ELSE
				    v_table_ddl := 'CREATE TABLE ' || in_schema || '.' || in_table || ' PARTITION OF ' || in_schema || '.' || v_parent || ' ' || v_partbound || ' ' || v_tablespace || '; ' || E'\n';
				  END IF;
        END IF;
        -- Jump to constraints and index section to add the check constraints and indexes and perhaps FKeys
      END IF;
    END IF;
	IF bVerbose THEN RAISE INFO '(1)tabledef so far: %', v_table_ddl; END IF;

    IF NOT bPartition THEN
      -- see if this is unlogged or temporary table
      select c.relpersistence into v_persist from pg_class c, pg_namespace n where n.nspname = in_schema and n.oid = c.relnamespace and c.relname = in_table and c.relkind = 'r';
      IF v_persist = 'u' THEN
        v_temp := 'UNLOGGED';
      ELSIF v_persist = 't' THEN
        v_temp := 'TEMPORARY';
      ELSE
        v_temp := '';
      END IF;
    END IF;
    
    -- start the create definition for regular tables unless we are in progress creating an inheritance-based child table
    IF NOT bPartition THEN
      --Issue#17 fix for case-sensitive tables
      -- SELECT count(*) INTO v_cnt1 FROM information_schema.tables t WHERE EXISTS (SELECT REGEXP_MATCHES(s.table_name, '([A-Z]+)','g') FROM information_schema.tables s 
      -- WHERE t.table_schema=s.table_schema AND t.table_name=s.table_name AND t.table_schema = quote_ident(in_schema) AND t.table_name = quote_ident(in_table) AND t.table_type = 'BASE TABLE');   
      SELECT count(*) INTO v_cnt1 FROM information_schema.tables t WHERE EXISTS (SELECT REGEXP_MATCHES(s.table_name, '([A-Z]+)','g') FROM information_schema.tables s 
      WHERE t.table_schema=s.table_schema AND t.table_name=s.table_name AND t.table_schema = in_schema AND t.table_name = in_table AND t.table_type = 'BASE TABLE');         
      IF v_cnt1 > 0 THEN
        v_table_ddl := 'CREATE ' || v_temp || ' TABLE ' || in_schema || '."' || in_table || '" (' || E'\n';
      ELSE
        v_table_ddl := 'CREATE ' || v_temp || ' TABLE ' || in_schema || '.' || in_table || ' (' || E'\n';
      END IF;
    END IF;
    -- RAISE INFO 'DEBUG2: tabledef so far: %', v_table_ddl;    
    -- define all of the columns in the table unless we are in progress creating an inheritance-based child table
    IF NOT bPartition THEN
      FOR v_colrec IN
        SELECT c.column_name, c.data_type, c.udt_name, c.udt_schema, c.character_maximum_length, c.is_nullable, c.column_default, c.numeric_precision, c.numeric_scale, c.is_identity, c.identity_generation        
        FROM information_schema.columns c WHERE (table_schema, table_name) = (in_schema, in_table) ORDER BY ordinal_position
      LOOP
         IF bVerbose THEN RAISE INFO '(col loop) name=% type=% udt_name=% udt_schema=%', v_colrec.column_name, v_colrec.data_type, v_colrec.udt_name, v_colrec.udt_schema; END IF;  
         -- v17 fix: handle case-sensitive for pg_get_serial_sequence that requires SQL Identifier handling
         -- SELECT CASE WHEN pg_get_serial_sequence(v_qualified, v_colrec.column_name) IS NOT NULL THEN True ELSE False END into bSerial;
         SELECT CASE WHEN pg_get_serial_sequence(quote_ident(in_schema) || '.' || quote_ident(in_table), v_colrec.column_name) IS NOT NULL THEN True ELSE False END into bSerial;
         IF bVerbose THEN
           -- v17 fix: handle case-sensitive for pg_get_serial_sequence that requires SQL Identifier handling
           -- SELECT pg_get_serial_sequence(v_qualified, v_colrec.column_name) into v_temp;
           SELECT pg_get_serial_sequence(quote_ident(in_schema) || '.' || quote_ident(in_table), v_colrec.column_name) into v_temp;
           IF v_temp IS NULL THEN v_temp = 'NA'; END IF;
           SELECT public.pg_get_coldef(in_schema, in_table,v_colrec.column_name) INTO v_diag1;
           --RAISE NOTICE 'DEBUG table: %  Column: %  datatype: %  Serial=%  serialval=%  coldef=%', v_qualified, v_colrec.column_name, v_colrec.data_type, bSerial, v_temp, v_diag1;
           --RAISE NOTICE 'DEBUG tabledef: %', v_table_ddl;
         END IF;
         
         --Issue#17 put double-quotes around case-sensitive column names
         SELECT COUNT(*) INTO v_cnt1 FROM information_schema.columns t WHERE EXISTS (SELECT REGEXP_MATCHES(s.column_name, '([A-Z]+)','g') FROM information_schema.columns s 
         WHERE t.table_schema=s.table_schema and t.table_name=s.table_name and t.column_name=s.column_name AND t.table_schema = quote_ident(in_schema) AND column_name = v_colrec.column_name);         

         --Issue#19 put double-quotes around SQL keyword column names         
         SELECT COUNT(*) INTO v_cnt2 FROM pg_get_keywords() WHERE word = v_colrec.column_name AND catcode = 'R';
         
         IF v_cnt1 > 0 OR v_cnt2 > 0 THEN
           v_table_ddl := v_table_ddl || '  "' || v_colrec.column_name || '" ';
         ELSE
           v_table_ddl := v_table_ddl || '  ' || v_colrec.column_name || ' ';
         END IF;
         
         v_table_ddl := v_table_ddl ||
         CASE WHEN v_colrec.udt_name in ('geometry', 'box2d', 'box2df', 'box3d', 'geography', 'geometry_dump', 'gidx', 'spheroid', 'valid_detail')
		       THEN v_colrec.udt_name 
		     WHEN v_colrec.data_type = 'USER-DEFINED' 
		       THEN v_colrec.udt_schema || '.' || v_colrec.udt_name 
		     WHEN v_colrec.data_type = 'ARRAY' 
   		     -- Issue#6 fix: handle arrays
		       THEN public.pg_get_coldef(in_schema, in_table,v_colrec.column_name) 
         -- v17 fix: handle case-sensitive for pg_get_serial_sequence that requires SQL Identifier handling
		     -- WHEN pg_get_serial_sequence(v_qualified, v_colrec.column_name) IS NOT NULL 
		     WHEN pg_get_serial_sequence(quote_ident(in_schema) || '.' || quote_ident(in_table), v_colrec.column_name) IS NOT NULL
		       -- Issue#8 fix: handle serial. Note: NOT NULL is implied so no need to declare it explicitly
		       THEN public.pg_get_coldef(in_schema, in_table,v_colrec.column_name)  
		     ELSE v_colrec.data_type END 
         || CASE WHEN v_colrec.is_identity = 'YES' THEN CASE WHEN v_colrec.identity_generation = 'ALWAYS' THEN ' GENERATED ALWAYS AS IDENTITY' ELSE ' GENERATED BY DEFAULT AS IDENTITY' END ELSE '' END
         || CASE WHEN v_colrec.character_maximum_length IS NOT NULL THEN ('(' || v_colrec.character_maximum_length || ')') 
                 WHEN v_colrec.numeric_precision > 0 AND v_colrec.numeric_scale > 0 THEN '(' || v_colrec.numeric_precision || ',' || v_colrec.numeric_scale || ')' 
                 ELSE '' END || ' '
		     || CASE WHEN bSerial THEN '' ELSE CASE WHEN v_colrec.is_nullable = 'NO' THEN 'NOT NULL' ELSE 'NULL' END END 
         || CASE WHEN bSerial THEN '' ELSE CASE WHEN v_colrec.column_default IS NOT null THEN (' DEFAULT ' || v_colrec.column_default) ELSE '' END END 
         || ',' || E'\n';
      END LOOP;
    END IF;
    IF bVerbose THEN RAISE INFO '(2)tabledef so far: %', v_table_ddl; END IF;
    
    -- define all the constraints: conparentid does not exist pre PGv11
    IF v_pgversion < 110000 THEN
      FOR v_constraintrec IN
        SELECT con.conname as constraint_name, con.contype as constraint_type,
          CASE
            WHEN con.contype = 'p' THEN 1 -- primary key constraint
            WHEN con.contype = 'u' THEN 2 -- unique constraint
            WHEN con.contype = 'f' THEN 3 -- foreign key constraint
            WHEN con.contype = 'c' THEN 4
            ELSE 5
          END as type_rank,
          pg_get_constraintdef(con.oid) as constraint_definition
        FROM pg_catalog.pg_constraint con JOIN pg_catalog.pg_class rel ON rel.oid = con.conrelid JOIN pg_catalog.pg_namespace nsp ON nsp.oid = connamespace
        WHERE nsp.nspname = in_schema AND rel.relname = in_table ORDER BY type_rank
        LOOP
        IF v_constraintrec.type_rank = 1 THEN
            v_primary := True;
            IF pkcnt = 0 THEN
                v_constraint_name := v_constraintrec.constraint_name;
                v_constraint_def  := v_constraintrec.constraint_definition;
            ELSE
              -- Issue#16 handle external PG def
              v_constraint_name := v_constraintrec.constraint_name;
              SELECT 'ALTER TABLE ONLY ' || in_schema || '.' || c.relname || ' ADD CONSTRAINT ' || r.conname || ' ' || pg_catalog.pg_get_constraintdef(r.oid, true) || ';' INTO v_pkey_def 
              FROM pg_catalog.pg_constraint r, pg_class c, pg_namespace n where r.conrelid = c.oid and  r.contype = 'p' and n.oid = r.connamespace and n.nspname = in_schema AND c.relname = in_table;              
            END IF;
            IF bPartition THEN
              continue;
            END IF;
        ELSE
            v_constraint_name := v_constraintrec.constraint_name;
            v_constraint_def  := v_constraintrec.constraint_definition;
        END IF;
        if bVerbose THEN RAISE INFO 'DEBUG4: constraint name=% constraint_def=%', v_constraint_name,v_constraint_def; END IF;
        constraintarr := constraintarr || v_constraintrec.constraint_name:: text;
  
        IF fktype <> 'FKEYS_INTERNAL' AND v_constraintrec.constraint_type = 'f' THEN
            continue;
        END IF;
        
        IF pkcnt = 0 THEN
          v_table_ddl := v_table_ddl || '  ' -- note: two char spacer to start, to indent the column
            || 'CONSTRAINT' || ' '
            || v_constraint_name || ' '
            || v_constraint_def
            || ',' || E'\n';
        END IF;
      END LOOP;
    
    ELSE
      FOR v_constraintrec IN
        SELECT con.conname as constraint_name, con.contype as constraint_type,
          CASE
            WHEN con.contype = 'p' THEN 1 -- primary key constraint
            WHEN con.contype = 'u' THEN 2 -- unique constraint
            WHEN con.contype = 'f' THEN 3 -- foreign key constraint
            WHEN con.contype = 'c' THEN 4
            ELSE 5
          END as type_rank,
          pg_get_constraintdef(con.oid) as constraint_definition
        FROM pg_catalog.pg_constraint con JOIN pg_catalog.pg_class rel ON rel.oid = con.conrelid JOIN pg_catalog.pg_namespace nsp ON nsp.oid = connamespace
        WHERE nsp.nspname = in_schema AND rel.relname = in_table 
              --Issue#13 added this condition:
              AND con.conparentid = 0 
              ORDER BY type_rank
        LOOP
        IF v_constraintrec.type_rank = 1 THEN
            v_primary := True;
            IF pkcnt = 0 THEN
                v_constraint_name := v_constraintrec.constraint_name;
                v_constraint_def  := v_constraintrec.constraint_definition;
            ELSE
              -- Issue#16 handle external PG def
              v_constraint_name := v_constraintrec.constraint_name;
              SELECT 'ALTER TABLE ONLY ' || in_schema || '.' || c.relname || ' ADD CONSTRAINT ' || r.conname || ' ' || pg_catalog.pg_get_constraintdef(r.oid, true) || ';' INTO v_pkey_def 
              FROM pg_catalog.pg_constraint r, pg_class c, pg_namespace n where r.conrelid = c.oid and  r.contype = 'p' and n.oid = r.connamespace and n.nspname = in_schema AND c.relname = in_table;              
            END IF;
            IF bPartition THEN
              continue;
            END IF;           
        ELSE
            v_constraint_name := v_constraintrec.constraint_name;
            v_constraint_def  := v_constraintrec.constraint_definition;
        END IF;
        -- SELECT 'ALTER TABLE ONLY ' || c.relname || ' ADD CONSTRAINT ' || r.conname || ' ' || pg_catalog.pg_get_constraintdef(r.oid, true) || ';' as pkeyddl FROM pg_catalog.pg_constraint r, pg_class c, pg_namespace n where r.conrelid = c.oid and  r.contype = 'p' and n.oid = r.connamespace and n.nspname = 'sample' AND c.relname = 'extensions_table';
        if bVerbose THEN RAISE INFO 'DEBUG4: constraint name=% constraint_def=%', v_constraint_name,v_constraint_def; END IF;
        constraintarr := constraintarr || v_constraintrec.constraint_name:: text;
  
        IF fktype <> 'FKEYS_INTERNAL' AND v_constraintrec.constraint_type = 'f' THEN
            continue;
        END IF;
  
        IF pkcnt = 0 THEN
          v_table_ddl := v_table_ddl || '  ' -- note: two char spacer to start, to indent the column
            || 'CONSTRAINT' || ' '
            || v_constraint_name || ' '
            || v_constraint_def
            || ',' || E'\n';
        END IF;
      END LOOP;
    END IF;      
    IF bVerbose THEN RAISE INFO '(3)tabledef so far: %', v_table_ddl; END IF;
	
    -- drop the last comma before ending the create statement
    v_table_ddl = substr(v_table_ddl, 0, length(v_table_ddl) - 1) || E'\n';

    -- ---------------------------------------------------------------------------
    -- at this point we have everything up to the last table-enclosing parenthesis
    -- ---------------------------------------------------------------------------
    IF bVerbose THEN RAISE INFO '(4)tabledef so far: %', v_table_ddl; END IF;

    -- See if this is an inheritance-based child table and finish up the table create.
    IF bPartition and bInheritance THEN
      -- Issue#11: handle parent schema
      -- v_table_ddl := v_table_ddl || ') INHERITS (' || in_schema || '.' || v_parent || ') ' || E'\n' || v_relopts || ' ' || v_tablespace || ';' || E'\n';
      IF v_parent_schema = '' OR v_parent_schema IS NULL THEN v_parent_schema = in_schema; END IF;
      v_table_ddl := v_table_ddl || ') INHERITS (' || v_parent_schema || '.' || v_parent || ') ' || E'\n' || v_relopts || ' ' || v_tablespace || ';' || E'\n';
    END IF;

    IF v_pgversion >= 100000 AND NOT bPartition and NOT bInheritance THEN
      -- See if this is a partitioned table (pg_class.relkind = 'p') and add the partitioned key 
      SELECT pg_get_partkeydef(c1.oid) as partition_key INTO v_partition_key FROM pg_class c1 JOIN pg_namespace n ON (n.oid = c1.relnamespace) LEFT JOIN pg_partitioned_table p ON (c1.oid = p.partrelid) 
      WHERE n.nspname = in_schema and n.oid = c1.relnamespace and c1.relname = in_table and c1.relkind = 'p';

      IF v_partition_key IS NOT NULL AND v_partition_key <> '' THEN
        -- add partition clause
        -- NOTE:  cannot specify default tablespace for partitioned relations
        -- v_table_ddl := v_table_ddl || ') PARTITION BY ' || v_partition_key || ' ' || v_tablespace || ';' || E'\n';  
        v_table_ddl := v_table_ddl || ') PARTITION BY ' || v_partition_key || ';' || E'\n';  
      ELSEIF v_relopts <> '' THEN
        v_table_ddl := v_table_ddl || ') ' || v_relopts || ' ' || v_tablespace || ';' || E'\n';  
      ELSE
        -- end the create definition
        v_table_ddl := v_table_ddl || ') ' || v_tablespace || ';' || E'\n';    
      END IF;  
    END IF;

    IF bVerbose THEN RAISE INFO '(5)tabledef so far: %', v_table_ddl; END IF;
    
    -- Add closing paren for regular tables
    -- IF NOT bPartition THEN
    -- v_table_ddl := v_table_ddl || ') ' || v_relopts || ' ' || v_tablespace || E';\n';  
    -- END IF;
    -- RAISE NOTICE 'ddlsofar3: %', v_table_ddl;

    -- Issue#16 create the external PKEY def if indicated
    IF v_pkey_def <> '' THEN
        v_table_ddl := v_table_ddl || v_pkey_def || E'\n';    
    END IF;
   
    IF bVerbose THEN RAISE INFO '(6)tabledef so far: %', v_table_ddl; END IF;
   
    -- create indexes
    FOR v_indexrec IN
      SELECT indexdef, COALESCE(tablespace, 'pg_default') as tablespace, indexname FROM pg_indexes WHERE (schemaname, tablename) = (in_schema, in_table)
    LOOP
      -- RAISE INFO 'DEBUG6: indexname=%', v_indexrec.indexname;             
      -- loop through constraints and skip ones already defined
      bSkip = False;
      FOREACH constraintelement IN ARRAY constraintarr
      LOOP 
         IF constraintelement = v_indexrec.indexname THEN
             -- RAISE INFO 'DEBUG7: skipping index, %', v_indexrec.indexname;
             bSkip = True;
             EXIT;
         END IF;
      END LOOP;   
      if bSkip THEN CONTINUE; END IF;
      
      -- Add IF NOT EXISTS clause so partition index additions will not be created if declarative partition in effect and index already created on parent
      v_indexrec.indexdef := REPLACE(v_indexrec.indexdef, 'CREATE INDEX', 'CREATE INDEX IF NOT EXISTS');
      -- RAISE INFO 'DEBUG8: adding index, %', v_indexrec.indexname;
      
      -- NOTE:  cannot specify default tablespace for partitioned relations
      IF v_partition_key IS NOT NULL AND v_partition_key <> '' THEN
          v_table_ddl := v_table_ddl || v_indexrec.indexdef || ';' || E'\n';
      ELSE
          v_table_ddl := v_table_ddl || v_indexrec.indexdef || ' TABLESPACE ' || v_indexrec.tablespace || ';' || E'\n';
      END IF;
      
    END LOOP;
    IF bVerbose THEN RAISE INFO '(7)tabledef so far: %', v_table_ddl; END IF;
    
    -- Handle external foreign key defs here if applicable. 
    IF fktype = 'FKEYS_EXTERNAL' THEN
      -- Issue#13 fix here too for conparentid = 0. and had to change to a loop to handle multiple return set, not a select into variable syntax.
      -- Also had to account for PG V10 where there is no conparentid
      IF v_pgversion < 110000 THEN
        FOR v_constraintrec IN
        SELECT 'ALTER TABLE ONLY ' || n.nspname || '.' || c2.relname || ' ADD CONSTRAINT ' || r.conname || ' ' || pg_catalog.pg_get_constraintdef(r.oid, true) || ';' as fkeydef
        FROM pg_constraint r, pg_class c1, pg_namespace n, pg_class c2 where r.conrelid = c1.oid and  r.contype = 'f' and n.nspname = in_schema and n.oid = r.connamespace and r.conrelid = c2.oid and c2.relname = in_table 
        LOOP
          v_table_ddl := v_table_ddl || v_constraintrec.fkeydef || ';' || E'\n';
          IF bVerbose THEN RAISE INFO 'keydef = %', v_constraintrec.fkeydef; END IF;
        END LOOP;            
      ELSE
        FOR v_constraintrec IN
        SELECT 'ALTER TABLE ONLY ' || n.nspname || '.' || c2.relname || ' ADD CONSTRAINT ' || r.conname || ' ' || pg_catalog.pg_get_constraintdef(r.oid, true) || ';' as fkeydef
        FROM pg_constraint r, pg_class c1, pg_namespace n, pg_class c2 where r.conrelid = c1.oid and  r.contype = 'f' and n.nspname = in_schema and n.oid = r.connamespace and r.conrelid = c2.oid and c2.relname = in_table and r.conparentid = 0
        LOOP
          v_table_ddl := v_table_ddl || v_constraintrec.fkeydef || E'\n';
          IF bVerbose THEN RAISE INFO 'keydef = %', v_constraintrec.fkeydef; END IF;
        END LOOP;            
      END IF;
      
    ELSIF  fktype = 'FKEYS_COMMENTED' THEN 
      SELECT '-- ALTER TABLE ONLY ' || n.nspname || '.' || c2.relname || ' ADD CONSTRAINT ' || r.conname || ' ' || pg_catalog.pg_get_constraintdef(r.oid, true) || ';' into v_fkey_defs 
      FROM pg_constraint r, pg_class c1, pg_namespace n, pg_class c2 where r.conrelid = c1.oid and  r.contype = 'f' and n.nspname = in_schema and n.oid = r.connamespace and r.conrelid = c2.oid and c2.relname = in_table;
	  IF v_fkey_defs IS NOT NULL THEN
          v_table_ddl := v_table_ddl || v_fkey_defs;
      END IF;
    END IF;
    IF bVerbose THEN RAISE INFO '(8)tabledef so far: %', v_table_ddl; END IF;
	
    IF trigtype = 'INCLUDE_TRIGGERS' THEN
	    -- Issue#14: handle multiple triggers for a table
      FOR v_trigrec IN
          select pg_get_triggerdef(t.oid, True) || ';' as triggerdef FROM pg_trigger t, pg_class c, pg_namespace n 
          WHERE n.nspname = in_schema and n.oid = c.relnamespace and c.relname = in_table and c.relkind = 'r' and t.tgrelid = c.oid and NOT t.tgisinternal
      LOOP
          v_table_ddl := v_table_ddl || v_trigrec.triggerdef;
          v_table_ddl := v_table_ddl || E'\n';          
          IF bVerbose THEN RAISE INFO 'triggerdef = %', v_trigrec.triggerdef; END IF;
      END LOOP;       	    
    END IF;
  
    -- add empty line
    v_table_ddl := v_table_ddl || E'\n';

    RETURN v_table_ddl;
	
    EXCEPTION
    WHEN others THEN
    BEGIN
      GET STACKED DIAGNOSTICS v_diag1 = MESSAGE_TEXT, v_diag2 = PG_EXCEPTION_DETAIL, v_diag3 = PG_EXCEPTION_HINT, v_diag4 = RETURNED_SQLSTATE, v_diag5 = PG_CONTEXT, v_diag6 = PG_EXCEPTION_CONTEXT;
      -- v_ret := 'line=' || v_diag6 || '. '|| v_diag4 || '. ' || v_diag1 || ' .' || v_diag2 || ' .' || v_diag3;
      v_ret := 'line=' || v_diag6 || '. '|| v_diag4 || '. ' || v_diag1;
      RAISE EXCEPTION '%', v_ret;
      -- put additional coding here if necessarY
       RETURN '';
    END;

  END;
$$;
