# VintaSoft WPF PDF Editor Demo

This C# project uses <a href="https://www.vintasoft.com/vsimaging-dotnet-index.html">VintaSoft Imaging .NET SDK</a> and demonstrates how to view and edit PDF document in WPF:
* Create and load PDF document.
* Display and print PDF document with or without color management.
* View and change general information about PDF document.
* View and change permission settings of PDF document.
* Change password of PDF document.
* Encrypt PDF document.
* Verify PDF document to conformance with PDF/A-1a, PDF/A-1b, PDF/A-2a, PDF/A-2b, PDF/A-3a, PDF/A-3b, PDF/A-4, PDF/A-4e or PDF/A-4f specification.
* Convert PDF document to PDF/A-1a, PDF/A-1b, PDF/A-2a, PDF/A-2b, PDF/A-3a, PDF/A-3b, PDF/A-4, PDF/A-4e or PDF/A-4f format.
* View, validate, add, remove digital signatures. Sign PDF document.
* View, add, remove, edit annotations.
* View, add, remove, edit, fill interactive form fields.
* Navigate PDF document.
* Execute JavaScript actions of PDF document.
* Add, reorder, remove pages of PDF document.
* Save PDF page to any supported image file format.
* Extract text and images from PDF page.
* Draw figures (text, image, rectangle, ellipse, lines, curves, polygons) on PDF page.
* Remove content of PDF document.
* Select text on PDF page.
* Obfuscate text in PDF document.
* Add, edit, reorder, remove bookmarks of PDF document.
* View and edit annotations of PDF document.
* View and edit the interactive fields of PDF document.
* Fill, export and import the interactive form of PDF document.
* View, generate and remove embedded thumbnails of PDF pages.
* View, add and remove embedded files of PDF document.
* View and compress image resources of PDF document.
* View information about fonts of PDF document.
* Save embedded files or image resources to a file.
* Pack PDF document.
* Save changes in PDF document.
* Convert PDF document to a TIFF file.


## Screenshot
<img src="vintasoft-wpf-pdf-editor-demo.png" title="VintaSoft WPF PDF Editor Demo">


## Usage
1. Get the 30 day free evaluation license for <a href="https://www.vintasoft.com/vsimaging-dotnet-index.html" target="_blank">VintaSoft Imaging .NET SDK</a> as described here: <a href="https://www.vintasoft.com/docs/vsimaging-dotnet/Licensing-Evaluation.html" target="_blank">https://www.vintasoft.com/docs/vsimaging-dotnet/Licensing-Evaluation.html</a>

2. Update the evaluation license in "CSharp\MainWindow.xaml.cs" file:
   ```
   Vintasoft.Imaging.ImagingGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");
   ```

3. Build the project ("WpfPdfEditorDemo.Net7.csproj" file) in Visual Studio or using .NET CLI:
   ```
   dotnet build WpfPdfEditorDemo.Net7.csproj
   ```

4. Run compiled application and try to view and edit PDF document.


## Documentation
VintaSoft Imaging .NET SDK on-line User Guide and API Reference for .NET developer is available here: https://www.vintasoft.com/docs/vsimaging-dotnet/


## Support
Please visit our <a href="https://myaccount.vintasoft.com/">online support center</a> if you have any question or problem.
