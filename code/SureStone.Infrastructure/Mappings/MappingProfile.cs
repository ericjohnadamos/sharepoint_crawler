namespace SureStone.Infrastructure.Mappings;

using AutoMapper;
using SureStone.Domain.Entities;
using SureStone.Infrastructure.Extensions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Dictionary<string, object>, CrawledFiles>()
            .ForMember(
                destination => destination.FilePath,
                option => option.MapFrom(source => source.GetValueOrDefault("FileRef")))
            .ForMember(
                destination => destination.FileDirectory,
                option => option.MapFrom(source => source.GetValueOrDefault("FileDirRef")))
            .ForMember(
                destination => destination.FileName,
                option => option.MapFrom(source => source.GetValueOrDefault("FileLeafRef")))
            .ForMember(
                destination => destination.MimeType,
                option => option.MapFrom(source => source.GetValueOrDefault("File_x0020_Type")))
            .ForMember(
                destination => destination.IsMicrosoftExtension,
                option => option.MapFrom(
                    source => source.GetValueOrDefault("File_x0020_Type").IsMicrosoftSupportedExtensionAsString()))
            .ForMember(
                destination => destination.CreationDateTime,
                option => option.MapFrom(source => source.GetValueOrDefault("Created")))
            .ForMember(
                destination => destination.LastModifiedDateTime,
                option => option.MapFrom(source => source.GetValueOrDefault("Modified")));
    }
}
