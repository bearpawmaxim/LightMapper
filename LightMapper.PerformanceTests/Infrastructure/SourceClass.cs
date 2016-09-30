﻿using System;

namespace LightMapper.PerformanceTests.Infrastructure
{
    public class SourceClass
    {
        public int IntProp { get; set; }
        public bool BoolProp { get; set; }
        public DateTime DateTimeProp { get; set; }

        public string StringField;
        public Guid GuidField;
        public TimeSpan TimeSpanProp { get; set; }

        public bool DiffProp1 { get; set; }
        public decimal DiffField1 { get; set; }
        public DateTime? NullProp { get; set; }
        public TimeSpan? NullField { get; set; }

        public string SProp { get; set; }

        public TestEnum EnumProp { get; set; }
        public int EnumField;
    }

    public enum TestEnum
    {
        EnumItem0,
        EnumItem1,
        EnumItem2,
        EnumItem3,
        EnumItem4,
        EnumItem5,
        EnumItem6,
        EnumItem7,
        EnumItem8,
        EnumItem9s
    }
}