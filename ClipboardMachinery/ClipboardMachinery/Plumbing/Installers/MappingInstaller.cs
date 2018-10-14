using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.Repositories.Shema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Plumbing.Installers {

    public class MappingInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            MapperConfiguration config = new MapperConfiguration(cfg => {
                cfg.CreateMap<Clip, ClipModel>();

                cfg.CreateMap<Tag, TagModel>()
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(source => source.Type.Name));
            });

            container.Register(
                Component.For<IMapper>().Instance(config.CreateMapper())
            );
        }

    }

}
