using System.ComponentModel.DataAnnotations;

namespace LensmaniaLibrary.DTOs.Events;

public record SetCoverPhotoRequest([Range(1, int.MaxValue)] int PostId);
