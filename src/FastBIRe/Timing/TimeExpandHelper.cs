using FastBIRe.AAMode;
using FastBIRe.Naming;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Timing
{
    public class TimeExpandHelper : ITimeExpandHelper
    {
        public TimeExpandHelper(INameGenerator nameGenerator, ITimeNameMapper timeNameMapper)
        {
            NameGenerator = nameGenerator;
            TimeNameMapper = timeNameMapper;
        }

        public INameGenerator NameGenerator { get; }

        public ITimeNameMapper TimeNameMapper { get; }

        public IEnumerable<string> Create(string name, TimeTypes type)
        {
            var args = new string[2];
            args[0] = name;
            if ((type & TimeTypes.Second) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Second);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Minute) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Minute);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Hour) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Hour);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Day) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Day);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Week) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Week);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Month) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Month);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Quarter) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Quarter);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Year) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Year);
                yield return NameGenerator.Create(args);
            }
        }
    }
}
