using Microsoft.AspNetCore.StaticFiles;
using Microsoft.JSInterop;

namespace FileUpload.Services
{
	public interface IFileDownload
	{
		Task<List<string>> GetUploadedFilesAsync();
		Task DownloadFile(string url);
	}

	public class FileDownload : IFileDownload
	{
		private IWebHostEnvironment _webHostEnvironment;
		private readonly IJSRuntime _jsRuntime;

		public FileDownload(IWebHostEnvironment webHostEnvironment, IJSRuntime jsRuntime)
		{
			_webHostEnvironment = webHostEnvironment;
			_jsRuntime = jsRuntime;
		}

		public async Task<List<string>> GetUploadedFilesAsync()
		{
			var base64Urls = new List<string>();
			var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
			var files = Directory.GetFiles(uploadPath);

			if (files != null && files.Length > 0)
			{
				foreach (var file in files)
				{
					using (var fileInput = new FileStream(file, FileMode.Open, FileAccess.Read))
					{
						var memoryStream = new MemoryStream();
						await fileInput.CopyToAsync(memoryStream);

						var buffer = memoryStream.ToArray();
						var fileType = GetMimeTypeForFileExtension(file);
						base64Urls.Add($"data:{fileType}; base64,{Convert.ToBase64String(buffer)}");
					}
				}
			}

			return base64Urls;
		}

		public async Task DownloadFile(string url)
		{
			await _jsRuntime.InvokeVoidAsync("downloadFile", url);
		}

		private string GetMimeTypeForFileExtension(string filePath)
		{
			const string DefaultContentType = "application/octet-stream";

			var provider = new FileExtensionContentTypeProvider();

			if (!provider.TryGetContentType(filePath, out var contentType))
			{
				contentType = DefaultContentType;
			}

			return contentType;
		}
	}
}
