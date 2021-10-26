namespace advisor
{
    using advisor.Persistence;
    using AutoMapper;

    public class MappingProfile : Profile {
        public MappingProfile() {
            // owned entities
            CreateMap<DbCapacity, DbCapacity>();
            CreateMap<DbExit, DbExit>();
            CreateMap<DbFleetContent, DbFleetContent>();
            CreateMap<DbSailors, DbSailors>();
            CreateMap<DbSettlement, DbSettlement>();
            CreateMap<DbSkill, DbSkill>();
            CreateMap<DbTransportationLoad, DbTransportationLoad>();
            CreateMap<Item, Item>();
            CreateMap<DbUnitItem, DbUnitItem>();
            CreateMap<DbProductionItem, DbProductionItem>();
            CreateMap<DbStatItem, DbStatItem>();
            CreateMap<DbMarketItem, DbMarketItem>();

            // owner entities

            CreateMap<DbFaction, DbFaction>()
                .ForMember(x => x.TurnNumber, opt => opt.Ignore())
                .ForMember(x => x.Turn, opt => opt.Ignore())
                .ForMember(x => x.Events, opt => opt.Ignore());

            CreateMap<DbRegion, DbRegion>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.TurnNumber, opt => opt.Ignore())
                .ForMember(x => x.Turn, opt => opt.Ignore())
                .ForMember(x => x.Units, opt => opt.Ignore());

            CreateMap<DbStructure, DbStructure>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.TurnNumber, opt => opt.Ignore())
                .ForMember(x => x.Turn, opt => opt.Ignore())
                .ForMember(x => x.Region, opt => opt.Ignore())
                .ForMember(x => x.Units, opt => opt.Ignore());
        }
    }
}
