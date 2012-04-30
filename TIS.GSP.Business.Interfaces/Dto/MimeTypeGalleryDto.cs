using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
  [Table("gs_MimeTypeGallery")]
  public class MimeTypeGalleryDto
  {
    [Key]
    public int MimeTypeGalleryId
    {
      get;
      set;
    }

    public int FKGalleryId
    {
      get;
      set;
    }

    public int FKMimeTypeId
    {
      get;
      set;
    }

    public bool IsEnabled
    {
      get;
      set;
    }

    [ForeignKey("FKMimeTypeId")]
    public MimeTypeDto MimeType
    {
      get; 
      set;
    }
  }
}
