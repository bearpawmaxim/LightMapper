﻿using System;

namespace LightMapper.PerformanceTests.Infrastructure
{
    public class TargetClass
    {
        public int IntProp { get; set; }
        public bool BoolProp { get; set; }
        public DateTime DateTimeProp { get; set; }

        public string StringField;
        public Guid GuidField;
        public TimeSpan TimeSpanProp { get; set; }

        public bool DiffProp2 { get; set; }
        public decimal DiffField2 { get; set; }
        public DateTime? NullProp { get; set; }
        public TimeSpan? NullField { get; set; }

        public string SField;

        public int EnumProp { get; set; }
        public TestEnum EnumField;
    }
}
