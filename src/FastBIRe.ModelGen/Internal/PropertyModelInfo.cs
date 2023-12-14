using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FastBIRe.ModelGen.Internal
{
    internal class MethodModelInfo
    {
        public MethodModelInfo(IMethodSymbol symbol)
        {
            Symbol = symbol;
        }

        public IMethodSymbol Symbol { get; }
    }
    internal class PropertyModelInfo
    {
        public PropertyModelInfo(IPropertySymbol symbol)
        {
            Symbol = symbol;
            IsAutoNumber=symbol.HasAttribute(Consts.AutoNumberAttribute.FullName);

            var name = symbol.Name;
            var displayAttr = symbol.GetAttribute(Consts.ColumnNameAttribute.FullName);
            if (displayAttr!=null)
            {
                var displayName=displayAttr.GetByIndex<string>(0);
                if (displayName!=null)
                {
                    name = displayName;
                }
            }
            Name = name;

            var maxLengthAttr=symbol.GetAttribute(Consts.MaxLengthAttribute.FullName);
            if (maxLengthAttr!=null)
            {
                Length = maxLengthAttr.GetByIndex<int?>(0);
            }

            var dataTypeAttr = symbol.GetAttribute(Consts.DbTypeAttribute.FullName);
            if (dataTypeAttr != null)
            {
                var dataType = dataTypeAttr.GetByNamed<string>(Consts.DbTypeAttribute.DataType);
                if (dataType !=null)
                {
                    DataType = dataType;
                }
                else
                {
                    PropertyDbType = dataTypeAttr.GetByNamed<DbType>(Consts.DbTypeAttribute.DataType);
                }
            }
            else
            {
                PropertyDbType = GetDbTypeFromType(symbol.Type);
            }

            var decimalAttr= symbol.GetAttribute(Consts.DecimalAttribute.FullName);
            if (decimalAttr!=null)
            {
                Precision = decimalAttr.GetByIndex<int>(0);
                Scale = decimalAttr.GetByIndex<int>(1);
            }

            var nullableAttr = symbol.GetAttribute(Consts.RequiredAttribute.FullName);
            if (nullableAttr!= null )
            {
                nullable = false;
            }
            else
            {
                nullable = IsNullableT || symbol.Type.IsReferenceType;
            }

            var idAttr = symbol.GetAttribute(Consts.IdAttribute.FullName);
            if (idAttr != null)
            {
                Id = idAttr.GetByIndex<object>(0);
            }

            var autoNumberAttr = symbol.GetAttribute(Consts.AutoNumberAttribute.FullName);
            if (autoNumberAttr != null)
            {
                IsAutoNumber = true;
                IdentityByDefault = autoNumberAttr.GetByNamed<bool>(Consts.AutoNumberAttribute.IdentityByDefault);
                IdentitySeed = autoNumberAttr.GetByNamed<long>(Consts.AutoNumberAttribute.IdentitySeed);
                IdentityIncrement = autoNumberAttr.GetByNamed<long>(Consts.AutoNumberAttribute.IdentityIncrement);
            }

            IsKey = symbol.HasAttribute(Consts.KeyAttribute.FullName);

            var indexAttr = symbol.GetAttribute(Consts.IndexAttribute.FullName);
            if (indexAttr!=null)
            {
                IsIndex = true;
                IndexOrder = indexAttr.GetByNamed<int>("Order");
                IndexName = indexAttr.GetByNamed<string>("IndexName");
                IndexGroup = indexAttr.NamedArguments.Any(x => x.Key == "IndexGroup") ? indexAttr.GetByNamed<int>("IndexGroup") : int.MinValue;
                IndexIsDesc= indexAttr.GetByNamed<bool>("IsDesc");
            }
        }

        private bool? nullable;

        public object? Id { get; }

        public bool IsKey { get; }

        public bool IsIndex { get; }

        public string? IndexName { get; }

        public int IndexOrder { get; }

        public int IndexGroup { get; }

        public bool IndexIsDesc { get; }

        public IPropertySymbol Symbol { get; }

        public bool IsAutoNumber { get; }

        public bool IdentityByDefault { get; set; }

        public long IdentitySeed { get; set; }

        public long IdentityIncrement { get; set; }

        public bool Nullable => nullable ?? false;

        public string Name { get; }

        public int? Length { get; }

        public string? DataType { get; }

        public DbType? PropertyDbType { get; }

        public bool IsCustomDataType => !string.IsNullOrEmpty(DataType);

        public int Precision { get; }

        public int Scale { get; }

        public bool IsNullableT => Symbol.Type.OriginalDefinition?.ToString() == "System.Nullable<T>";

        public bool IsSupportType
        {
            get
            {
                if (IsCustomDataType)
                {
                    return true;
                }
                return GetDbTypeFromType(Symbol.Type) != null;
            }
        }

        public string WriteBuildColumn(string tableBuildName)
        {
            var args = new List<string>
            {
                $"id:{Id.ObjectToCsharp()}",
                $"nullable:{Nullable.ToBoolKeyword()}",
                $"length:{Length.NullableToCSharp()}",
                $"scale:{Scale}",
                $"precision:{Precision}",
                $"isAutoNumber:{IsAutoNumber.ToBoolKeyword()}",
                $"identityByDefault:{IdentityByDefault.ToBoolKeyword()}",
                $"identitySeed:{IdentitySeed}",
                $"identityIncrement:{IdentityIncrement}",
            };

            var argJoined = string.Join(",", args);            
            var typeInput = string.IsNullOrEmpty(DataType) ? $"global::System.Data.DbType.{PropertyDbType!.Value}":$"\"{DataType}\"";

            var code = $"{tableBuildName}.Column(\"{Name}\", {typeInput}, {argJoined});";
            return code;
        }
        public WriteReadColumnResult WriteReadColumn(string recordName, string? prefx = null)
        {
            //TODO: GetOrdinal call once
            var ordinalVar = "ordinal" + Symbol.Name;
            var oridinalCall= $"var {ordinalVar} = {recordName}.GetOrdinal(\"{Name}\")";
            var writeCall = $"{prefx}{Symbol.Name} = {recordName}.IsDBNull({ordinalVar}) ? default: {GetRecordMethod(Symbol.Type, recordName, ordinalVar)}";
            return new WriteReadColumnResult(oridinalCall, writeCall);
        }


        public static string GetRecordMethod(ITypeSymbol symbol,string recordName,string ordinal)
        {
            switch (symbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return $"{recordName}.GetBoolean({ordinal})";
                case SpecialType.System_Byte:
                    return $"{recordName}.GetByte({ordinal})";
                case SpecialType.System_SByte:
                    return $"(sbyte){recordName}.GetByte({ordinal})";
                case SpecialType.System_Char:
                    return $"{recordName}.GetChar({ordinal})";
                case SpecialType.System_Int16:
                    return $"{recordName}.GetInt16({ordinal})";
                case SpecialType.System_UInt16:
                    return $"(ushort){recordName}.GetInt16({ordinal})";
                case SpecialType.System_Int32:
                    return $"{recordName}.GetInt32({ordinal})";
                case SpecialType.System_UInt32:
                    return $"(uint){recordName}.GetInt32({ordinal})";
                case SpecialType.System_Int64:
                    return $"{recordName}.GetInt64({ordinal})";
                case SpecialType.System_UInt64:
                    return $"(ulong){recordName}.GetInt64({ordinal})";
                case SpecialType.System_Single:
                    return $"{recordName}.GetFloat({ordinal})";
                case SpecialType.System_Double:
                    return $"{recordName}.GetDouble({ordinal})";
                case SpecialType.System_Decimal:
                    return $"{recordName}.GetDecimal({ordinal})";
                case SpecialType.System_String:
                    return $"{recordName}.GetString({ordinal})";
                case SpecialType.System_DateTime:
                    return $"{recordName}.GetDateTime({ordinal})";
                case SpecialType.System_Object:
                    return $"{recordName}.GetValue({ordinal})";
                default:
                    {
                        if (symbol.OriginalDefinition?.ToString() == "System.Nullable<T>")
                        {
                            return GetRecordMethod(((INamedTypeSymbol)symbol).TypeArguments[0], recordName, ordinal);
                        }
                        if (symbol.ToString() == "System.Guid")
                        {
                            return $"{recordName}.GetGuid({ordinal})";
                        }
                        throw new NotSupportedException(symbol.ToString());
                    }
            }
        }

        private DbType? GetDbTypeFromType(ITypeSymbol symbol)
        {
            switch (symbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return DbType.Boolean;
                case SpecialType.System_Byte:
                    return DbType.Byte;
                case SpecialType.System_SByte:
                    return DbType.SByte;
                case SpecialType.System_Char:
                    return DbType.Int16;
                case SpecialType.System_Int16:
                    return DbType.Int16;
                case SpecialType.System_UInt16:
                    return DbType.UInt16;
                case SpecialType.System_Int32:
                    return DbType.Int32;
                case SpecialType.System_UInt32:
                    return DbType.UInt32;
                case SpecialType.System_Int64:
                    return DbType.Int64;
                case SpecialType.System_UInt64:
                    return DbType.UInt64;
                case SpecialType.System_Single:
                    return DbType.Single;
                case SpecialType.System_Double:
                    return DbType.Double;
                case SpecialType.System_Decimal:
                    return DbType.Decimal;
                case SpecialType.System_String:
                    return DbType.String;
                case SpecialType.System_DateTime:
                    return DbType.DateTime;
                default:
                    {
                        if (IsNullableT)
                        {
                            nullable = true;
                            return GetDbTypeFromType(((INamedTypeSymbol)symbol).TypeArguments[0]);
                        }
                    }
                    return null;
            }
        }
    }
}
