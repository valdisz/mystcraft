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
            CreateMap<DbItem, DbItem>();
            CreateMap<DbSailors, DbSailors>();
            CreateMap<DbSettlement, DbSettlement>();
            CreateMap<DbSkill, DbSkill>();
            CreateMap<DbTradableItem, DbTradableItem>();
            CreateMap<DbTransportationLoad, DbTransportationLoad>();

            // owner entities

            CreateMap<DbFaction, DbFaction>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.TurnId, opt => opt.Ignore())
                .ForMember(x => x.Turn, opt => opt.Ignore())
                .ForMember(x => x.Events, opt => opt.Ignore());

            CreateMap<DbRegion, DbRegion>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.TurnId, opt => opt.Ignore())
                .ForMember(x => x.Turn, opt => opt.Ignore())
                .ForMember(x => x.Units, opt => opt.Ignore());

            CreateMap<DbStructure, DbStructure>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.TurnId, opt => opt.Ignore())
                .ForMember(x => x.RegionId, opt => opt.Ignore())
                .ForMember(x => x.Turn, opt => opt.Ignore())
                .ForMember(x => x.Region, opt => opt.Ignore())
                .ForMember(x => x.Units, opt => opt.Ignore());
        }
    }
}
