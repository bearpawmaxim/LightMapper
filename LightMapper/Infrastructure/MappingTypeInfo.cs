using System;

namespace LightMapper.Infrastructure
{
    /// <summary>Mapping type specific class</summary>
    public class MappingTypeInfo
    {
        /// <summary>Mapping type</summary>
        public Type Type { get; set; }
        /// <summary>Mapping type hash</summary>
        public int Hash { get; set; }
        /// <summary>Mapping type parent hash (if exists)</summary>
        public int? BaseHash { get; set; }
    }
}