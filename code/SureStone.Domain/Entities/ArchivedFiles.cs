#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor

namespace Insurance.Domain.Entities;

using System.ComponentModel.DataAnnotations.Schema;

[Table("archived_files")]
public class ArchivedFiles
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("file_path")]
    public string FilePath { get; set; }

    [Column("file_directory")]
    public string FileDirectory { get; set; }

    [Column("filename")]
    public string FileName { get; set; }

    [Column("is_done")]
    public bool IsDone { get; set; }

    [Column("file_not_found")]
    public bool FileNotFound { get; set; }
}
