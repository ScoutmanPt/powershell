using System.IO;
using System.Management.Automation;
using PnP.Framework.Provisioning.Providers;
using PnP.Framework.Provisioning.Providers.Xml;
using PnP.Framework.Provisioning.Providers.Markdown;
using PnP.PowerShell.Commands.Properties;
using PnP.PowerShell.Commands.Base;

namespace PnP.PowerShell.Commands.Provisioning.Site
{

    [Cmdlet(VerbsData.Convert, "SiteTemplateToMarkdown", SupportsShouldProcess = true)]

    public class ConvertSiteTemplateToMarkdown : BasePSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string TemplatePath;

        [Parameter(Mandatory = false)]
        public string Out;

        [Parameter(Mandatory = false)]
        public SwitchParameter Force;
        protected override void ExecuteCmdlet()
        {
            WriteWarning("This cmdlet is work in progress, the markdown report will improve/grow with later releases.");
            if (!Path.IsPathRooted(TemplatePath))
            {
                TemplatePath = System.IO.Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, TemplatePath);
            }

            if (!File.Exists(TemplatePath))
            {
                throw new PSArgumentException("File does not exist", nameof(TemplatePath));
            }


            if (ShouldProcess($"Converts a PnP Site Template to markdown format"))
            {
                var process = false;
                var template = ReadSiteTemplate.LoadSiteTemplateFromFile(TemplatePath, null, (exception) =>
                     {
                         throw new PSInvalidOperationException("Invalid template", exception);
                     });
                ITemplateFormatter mdFormatter = new MarkdownPnPFormatter();
                if (ParameterSpecified(nameof(Out)))
                {
                    if (!Path.IsPathRooted(Out))
                    {
                        Out = System.IO.Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, Out);
                    }
                    if (File.Exists(Out))
                    {
                        if (Force || ShouldContinue(string.Format(Resources.File0ExistsOverwrite, Out), Resources.Confirm))
                        {
                            process = true;
                        }
                    }
                    else
                    {
                        process = true;
                    }
                    if (process)
                    {
                        using (var outputStream = mdFormatter.ToFormattedTemplate(template))
                        {
                            using (var fileStream = File.Create(Out))
                            {
                                outputStream.Seek(0, SeekOrigin.Begin);
                                outputStream.CopyTo(fileStream);
                                fileStream.Close();
                            }
                        }
                    }
                }
                else
                {
                    using (var outputStream = mdFormatter.ToFormattedTemplate(template))
                    {
                        using (var reader = new StreamReader(outputStream))
                        {
                            WriteObject(reader.ReadToEnd());
                        }
                    }
                }
            }
        }
    }
}
