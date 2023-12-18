using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastBIRe
{
    public partial class DefaultScriptExecuter
    {
        private struct CommandState
        {
            public readonly DefaultScriptExecuter Executer;

            public readonly StackTrace? StackTrace;

            public readonly ScriptUnit? ScriptUnit;

            public readonly IEnumerable<ScriptUnit>? ScriptUnits;

            public long FullStartTime;

            public long? FullEndTime;

            public long ExecuteStartTime;

            public long? ExecuteEndTime;

            public readonly bool IsBatch;

            public readonly bool ScriptIsEmpty
            {
                get
                {
                    if (IsBatch)
                    {
                        return ScriptUnits!.Any(static x => IsEmptyScript(x.Script));
                    }
                    return IsEmptyScript(ScriptUnit!.Value.Script);
                }
            }

            public readonly TimeSpan? FullElapsedTime
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (FullEndTime == null)
                    {
                        return null;
                    }
                    return GetElapsedTime(FullStartTime, FullEndTime.Value);
                }
            }
            public readonly TimeSpan? ExecuteElapsedTime
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (ExecuteEndTime == null)
                    {
                        return null;
                    }
                    return GetElapsedTime(ExecuteStartTime, ExecuteEndTime.Value);
                }
            }

            public readonly TraceUnit Trace => new TraceUnit(ExecuteElapsedTime, FullElapsedTime, StackTrace);

            public CommandState(DefaultScriptExecuter executer, StackTrace? stackTrace, ScriptUnit scriptUnit)
            {
                Executer = executer;
                StackTrace = stackTrace;
                ScriptUnit = scriptUnit;
                ScriptUnits = new OneEnumerable<ScriptUnit>(scriptUnit);
                FullStartTime = Stopwatch.GetTimestamp();
                IsBatch = false;
            }
            public CommandState(DefaultScriptExecuter executer, StackTrace? stackTrace, IEnumerable<ScriptUnit> scriptUnits)
            {
                Executer = executer;
                StackTrace = stackTrace;
                ScriptUnit = null;
                ScriptUnits = scriptUnits;
                FullStartTime = Stopwatch.GetTimestamp();
                IsBatch = true;
            }

            public void RaiseBegin()
            {
                if (IsBatch)
                {
                    Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Begin(Executer.Connection, ScriptUnits!, Trace, Executer.dbTransaction));
                }
                else
                {
                    Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Begin(Executer.Connection, ScriptUnit!.Value, Trace, Executer.dbTransaction));
                }
            }
            public void RaiseCreatedCommand(DbCommand command)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.CreatedCommand(Executer.Connection, command, ScriptUnit!.Value, Trace, Executer.dbTransaction));
            }
            public void RaiseLoadCommand(DbCommand command)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.LoadCommand(Executer.Connection, command, ScriptUnit!.Value, Trace, Executer.dbTransaction));
            }
            public void RaiseSkip()
            {
                if (IsBatch)
                {
                    Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Skip(Executer.Connection, ScriptUnit!.Value, Trace, Executer.dbTransaction));
                }
                else
                {
                    Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Skip(Executer.Connection, ScriptUnits!, Trace, Executer.dbTransaction));
                }
            }
            public void RaiseSkip(ScriptUnit scriptUnit)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Skip(Executer.Connection, scriptUnit, Trace, Executer.dbTransaction));
            }
            public void RaiseExecuted(DbCommand command, int recordsAffected)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Executed(Executer.Connection, command, ScriptUnit!.Value, recordsAffected, Trace, Executer.dbTransaction));
            }
#if !NETSTANDARD2_0
            public void RaiseCreateBatch(DbBatch batch)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.CreatedBatch(Executer.Connection, ScriptUnits!, batch, Trace, Executer.dbTransaction));
            }
            public void RaiseLoadBatchItem(DbBatch batch, DbBatchCommand command)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.LoadBatchItem(Executer.Connection, ScriptUnits!, batch, command, Trace, Executer.dbTransaction));
            }
            public void RaiseExecutedBatch(DbBatch batch, int recordsAffected)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.ExecutedBatch(Executer.Connection, ScriptUnits!, batch, Trace, recordsAffected, Executer.dbTransaction));
            }
            public void LoadParamters(DbBatch batch, DbBatchCommand command, ScriptUnit unit)
            {
                command.CommandText = Executer.SqlQutoConversion(Executer.SqlParameterConversion(unit.Script));
                if (unit.Parameters == null)
                {
                    return;
                }
                //After https://github.com/dotnet/runtime/pull/89503
                //foreach (var item in unit.Parameters)
                //{
                //    command.Parameters[item.Key] = xxx;
                //}
                foreach (var item in unit.Parameters)
                {
                    command.Parameters.Add(item.Value);
                }
            }
#endif
            public void RaiseException(DbCommand? command
#if !NETSTANDARD2_0
                , DbBatch? batch
#endif
                , Exception exception)
            {
                if (IsBatch)
                {
#if !NETSTANDARD2_0
                    if (batch != null)
                    {
                        Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Exception(Executer.Connection, batch, ScriptUnits!, exception, Trace, Executer.dbTransaction));
                        return;
                    }
#endif
                    Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Exception(Executer.Connection, command!, ScriptUnits!, exception, Trace, Executer.dbTransaction));
                }
                else
                {
                    Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.Exception(Executer.Connection, command, ScriptUnit!.Value, exception, Trace, Executer.dbTransaction));
                }
            }
            public void RaiseStartReading(DbCommand command)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.StartReading(Executer.Connection, command, ScriptUnit!.Value, Trace, Executer.dbTransaction));
            }
            public void RaiseEndReading(DbCommand command)
            {
                Executer.ScriptStated?.Invoke(Executer, ScriptExecuteEventArgs.EndReading(Executer.Connection, command, ScriptUnit!.Value, Trace, Executer.dbTransaction));
            }
            public void LoadParamters(DbCommand command)
            {
                Debug.Assert(ScriptUnit != null);
                if (ScriptUnit!.Value.Parameters != null)
                {
                    foreach (var item in ScriptUnit!.Value.Parameters)
                    {
                        if (item.Value is DbParameter dbp)
                        {
                            command.Parameters.Add(dbp);
                        }
                        else
                        {
                            var par = command.CreateParameter();
                            par.ParameterName = item.Key;
                            par.Value = item.Value;
                            par.DbType = GetDbType(item.Value);
                            command.Parameters.Add(par);
                        }
                    }
                }
                command.CommandText = Executer.SqlQutoConversion(Executer.SqlParameterConversion(ScriptUnit!.Value.Script));
                command.CommandTimeout = Executer.CommandTimeout;
                command.Transaction = Executer.dbTransaction;
            }

            public void EndTime()
            {
                FullEndTime = ExecuteEndTime = Stopwatch.GetTimestamp();
            }
        }

    }
}
