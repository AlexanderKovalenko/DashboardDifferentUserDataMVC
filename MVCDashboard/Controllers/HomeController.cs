using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace MVCDashboard.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginData model) {
            HttpContext.Session["CurrentUser"] = model.UserName;
            return View("Dashboard");
        }

        // JSON data endpoint for the "JSON (custom filter expression)" dashboard
        public JsonResult GetCustomers(string CountryStartsWith) {
            var jsonText = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/customers_filter.json"));
            var result = JsonConvert.DeserializeObject<List<Customer>>(jsonText);

            if (!string.IsNullOrEmpty(CountryStartsWith))
                result = result.Where(customer => customer.Country.StartsWith(CountryStartsWith)).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}