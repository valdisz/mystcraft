namespace advisor.Schema {
    using advisor.Model;
    using HotChocolate.Types;

    public class SkillType : ObjectType<AnSkill> {
        protected override void Configure(IObjectTypeDescriptor<AnSkill> descriptor) {
            descriptor.Name("Skill");
        }
    }
}
