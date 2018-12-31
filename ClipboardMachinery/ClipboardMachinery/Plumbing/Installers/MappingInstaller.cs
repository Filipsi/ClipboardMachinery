using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.Repository.Schema;

namespace ClipboardMachinery.Plumbing.Installers {

    public class MappingInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            MapperConfiguration configuration = new MapperConfiguration(config => {
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
            });

            container.Register(
                Component.For<IMapper>().Instance(configuration.CreateMapper())
            );
        }

    }

}
