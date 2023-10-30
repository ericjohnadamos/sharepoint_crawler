#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor

namespace SureStone.Domain.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

[Table("crawled_files")]
public class CrawledFiles
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("file_path")]
    public string FilePath { get; set; }

    [Column("file_directory")]
    public string FileDirectory { get; set; }

    [Column("filename")]
    public string FileName { get; set; }

    [AllowNull]
    [Column("mime_type")]
    public string? MimeType { get; set; }

    [Column("is_microsoft")]
    public bool IsMicrosoftExtension { get; set; }

    [Column("creation_datetime")]
    public DateTime CreationDateTime { get; set; }

    [Column("last_modified_datetime")]
    public DateTime LastModifiedDateTime { get; set; }

    [AllowNull]
    [Column("origin_creation_datetime")]
    public DateTime? OriginCreationDateTime { get; set; }

    [AllowNull]
    [Column("origin_last_modified_datetime")]
    public DateTime? OriginLastModifiedDateTime { get; set; }
}
