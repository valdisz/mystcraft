namespace advisor.Schema;

using advisor.Model;
using advisor.Persistence;
using HotChocolate.Types;

public class ItemInterfaceType : InterfaceType<AnItem> {
    protected override void Configure(IInterfaceTypeDescriptor<AnItem> descriptor) {
        descriptor.Name("AnItem");
    }
}

public class ItemType : ObjectType<DbItem> {
    protected override void Configure(IObjectTypeDescriptor<DbItem> descriptor) {
        descriptor.Name("Item");
        descriptor.Implements<ItemInterfaceType>();
    }
}

public class UnitItemType : ObjectType<DbUnitItem> {
    protected override void Configure(IObjectTypeDescriptor<DbUnitItem> descriptor) {
        descriptor.Implements<ItemInterfaceType>();
    }
}

public class TradableItemType : ObjectType<DbTradableItem> {
    protected override void Configure(IObjectTypeDescriptor<DbTradableItem> descriptor) {
        descriptor.Implements<ItemInterfaceType>();
    }
}

public class BattleItemType : ObjectType<JBattleItem> {
    protected override void Configure(IObjectTypeDescriptor<JBattleItem> descriptor) {
        descriptor.Name("BattleItem");
        descriptor.Implements<ItemInterfaceType>();
    }
}

public class TurnStatisticsItemType : ObjectType<DbTurnStatisticsItem> {
    protected override void Configure(IObjectTypeDescriptor<DbTurnStatisticsItem> descriptor) {
        descriptor.Implements<ItemInterfaceType>();
    }
}

public class RegionStatisticsItemType : ObjectType<DbRegionStatisticsItem> {
    protected override void Configure(IObjectTypeDescriptor<DbRegionStatisticsItem> descriptor) {
        descriptor.Implements<ItemInterfaceType>();
    }
}

public class TreasuryItemType : ObjectType<DbTreasuryItem> {
    protected override void Configure(IObjectTypeDescriptor<DbTreasuryItem> descriptor) {
        descriptor.Implements<ItemInterfaceType>();
    }
}
