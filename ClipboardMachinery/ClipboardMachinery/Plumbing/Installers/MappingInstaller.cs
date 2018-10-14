using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Components.Clip;
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
                cfg
                    .CreateMap<Clip, ClipModel>()
                    .ForMember(dest => dest.Content, opt => opt.MapFrom(source => source.Content));
            });

            container.Register(
                Component.For<IMapper>().Instance(config.CreateMapper())
            );
        }

    }

}
