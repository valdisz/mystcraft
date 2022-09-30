namespace advisor;

using advisor.Persistence;
using AutoMapper;

public class MappingProfile : Profile {
    public MappingProfile() {
        CreateMap<DbCapacity, DbCapacity>();
        CreateMap<DbFleetContent, DbFleetContent>();
        CreateMap<DbSailors, DbSailors>();
        CreateMap<DbSettlement, DbSettlement>();
        CreateMap<DbSkill, DbSkill>();
        CreateMap<DbTransportationLoad, DbTransportationLoad>();
        CreateMap<DbItem, DbItem>();

        CreateMap<DbUnitItem, DbUnitItem>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
        ;

        CreateMap<DbTurnStatisticsItem, DbTurnStatisticsItem>()
            .ForMember(x => x.Id, opt => opt.Ignore())
        ;

        CreateMap<DbRegionStatisticsItem, DbRegionStatisticsItem>()
            .ForMember(x => x.Id, opt => opt.Ignore())
        ;

        CreateMap<DbTradableItem, DbTradableItem>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
        ;

        CreateMap<DbProductionItem, DbProductionItem>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
        ;

        CreateMap<DbTreasuryItem, DbTreasuryItem>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
        ;

        CreateMap<DbTurnStatisticsItem, DbTurnStatisticsItem>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
        ;

        CreateMap<DbRegionStatisticsItem, DbRegionStatisticsItem>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
        ;

        CreateMap<DbAttitude, DbAttitude>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
            .ForMember(x => x.Faction, opt => opt.Ignore())
        ;

        CreateMap<DbExit, DbExit>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
            .ForMember(x => x.Turn, opt => opt.Ignore())
            .ForMember(x => x.Origin, opt => opt.Ignore())
            .ForMember(x => x.Target, opt => opt.Ignore());

        CreateMap<DbFaction, DbFaction>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
            .ForMember(x => x.Turn, opt => opt.Ignore())
            .ForMember(x => x.Events, opt => opt.Ignore());

        CreateMap<DbRegion, DbRegion>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
            .ForMember(x => x.Turn, opt => opt.Ignore())
            .ForMember(x => x.Units, opt => opt.Ignore())
            .ForMember(x => x.ForSale, opt => opt.Ignore())
            .ForMember(x => x.Wanted, opt => opt.Ignore());

        CreateMap<DbStructure, DbStructure>()
            .ForMember(x => x.TurnNumber, opt => opt.Ignore())
            .ForMember(x => x.Turn, opt => opt.Ignore())
            .ForMember(x => x.Region, opt => opt.Ignore())
            .ForMember(x => x.Units, opt => opt.Ignore());
    }
}
