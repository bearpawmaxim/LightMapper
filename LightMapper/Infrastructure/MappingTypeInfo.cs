using System;

namespace LightMapper.Infrastructure
{
    public class MappingTypeInfo
    {
        public Type Type { get; set; }
        public int Hash { get; set; }
        public int? BaseHash { get; set; }
    }
}