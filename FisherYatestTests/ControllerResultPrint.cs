using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ControllerTest
{
    public static class ControllerResult
    {
        public static string Print(IActionResult result)
        {
            var sb = new StringBuilder("\n");

            
            if (result is ObjectResult obj)
            {
                sb.AppendLine($"StatusCode: {obj.StatusCode}");
                sb.AppendLine($"ContentTypes: {obj.ContentTypes}");
                sb.AppendLine($"Value: {JsonSerializer.Serialize(obj.Value)}");
            }
            else if (result is JsonResult json)
            {
                sb.AppendLine($"StatusCode: {json.StatusCode}");
                sb.AppendLine($"ContentType: {json.ContentType}");
                sb.AppendLine($"Content: {JsonSerializer.Serialize(json.Value)}");
            }
            else if (result is ContentResult content)
            {
                sb.AppendLine($"StatusCode: {content.StatusCode}");
                sb.AppendLine($"ContentType: {content.ContentType}");
                sb.AppendLine($"Content: {content.Content}");
            }

            return sb.ToString();
        }
    }
}
