using Microsoft.AspNetCore.Mvc;
using FisherYates;
using FisherYates.Services;
using FisherYates.Models;

namespace FisherYatesWebApp.Controllers
{
    public class FisherYates : Controller
    {
        private readonly IFisherYatesService _fisherYatesService;

        public FisherYates(IFisherYatesService fisherYatesService)
        {
            _fisherYatesService = fisherYatesService;
        }
        /// <summary>
        /// todo: Add the skeleton structure for the solution, and implement unit tests (in the FisherYatesTests project)!
        /// </summary>
        /// <param name="input">List of dasherized elements to shuffle (e.g. "D-B-A-C").</param>
        /// <returns>A "200 OK" HTTP response with a content-type of `text/plain; charset=utf-8`. The content should be the dasherized output of the algorithm, e.g. "C-D-A-B".</returns>
        [HttpGet("{input}")]
        public ActionResult Index([FromQuery] ValidRequest inputStr)
        {
            if (ModelState.IsValid)
            {
                var shuffledResult = _fisherYatesService.Shuffle(inputStr.input.ToString());
                return new ContentResult
                {
                    Content = shuffledResult,
                    ContentType = "text/plain; charset=utf-8",
                    StatusCode = 200
                };
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}