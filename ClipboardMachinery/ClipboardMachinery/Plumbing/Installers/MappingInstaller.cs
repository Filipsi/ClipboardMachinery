using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.Data.Schema;

namespace ClipboardMachinery.Plumbing.Installers {

    public class MappingInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            MapperConfiguration configuration = new MapperConfiguration(config => {
                // Mappings from database to view
                config
                    .CreateMap<Clip, ClipModel>();

                config
                    .CreateMap<Color, System.Windows.Media.Color>()
                    .ForMember(c => c.ScA, opt => opt.Ignore())
                    .ForMember(c => c.ScR, opt => opt.Ignore())
                    .ForMember(c => c.ScG, opt => opt.Ignore())
                    .ForMember(c => c.ScB, opt => opt.Ignore());

                config
                    .CreateMap<Tag, TagModel>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(source => source.Type.Name))
                    .ForMember(dest => dest.Color, opt => {
                        opt.MapFrom(source => source.Type.Color);
                        opt.AllowNull();
                    });

                config
                    .CreateMap<TagType, TagTypeModel>();

                // Mappings from view to database
                // NOTE: Experimental

                /*
                config
                    .CreateMap<ClipModel, Clip>()
                    .AfterMap((source, desct) => {
                        foreach (Tag tag in desct.Tags) {
                            tag.ClipId = desct.Id;
                        }
                    });

                config
                    .CreateMap<System.Windows.Media.Color, Color>();

                config
                    .CreateMap<TagModel, Tag>()
                    .ForMember(dest => dest.TypeName, opt => opt.MapFrom(source => source.Name))
                    .ForPath(dest => dest.Type.Name, opt => opt.MapFrom(source => source.Name))
                    .ForPath(dest => dest.Type.Color, opt => opt.MapFrom(source => source.Color))
                    .ForPath(dest => dest.Type.Type, opt => opt.MapFrom(source => source.Value.GetType()));
                */
            });

            container.Register(
                Component.For<IMapper>().Instance(configuration.CreateMapper())
            );
        }

    }

}
