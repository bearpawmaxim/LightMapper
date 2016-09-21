namespace LightMapper.Infrastructure
{
    /// <summary>Enum which describes explicit action execution order</summary>
    public enum ExplicitOrders : byte
    {
        /// <summary>Execute action before mapping</summary>
        BeforeMap,
        /// <summary>Execute action after mapping</summary>
        AfterMap
    }
}
