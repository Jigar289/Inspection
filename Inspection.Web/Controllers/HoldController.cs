using Inspection.Web.DataBase;
using Inspection.Web.Models;
using Inspection.Web.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inspection.Web.Controllers
{
    public class HoldController : Controller
    {
        // GET: Hold
        ITEIndiaEntities DB = new ITEIndiaEntities();
        LogService logService = new LogService();
        public ActionResult Index()
        {
            List<HoldProcessModel> _List = new List<HoldProcessModel>();
            try
            {
                // First filter using string comparison in SQL (works with EF)
                var rawData = DB.Final_Inspection_Process
                    .Where(p => p.Stage == "20") // Stage is a string in DB
                    .OrderByDescending(p => p.ID)
                    .Take(1000)
                    .ToList(); // Materialize the query (run it in SQL)

                // Now perform any .NET operations in memory (after .ToList)
                _List = rawData.Select(p => new HoldProcessModel
                {
                    ID = p.ID,
                    JobNum = p.JobNum,
                    PartNum = p.PartNum,
                    Inspection_Type = p.Inspection_Type,
                    Inspection_date = p.Inspection_date,
                    starttime = p.starttime,
                    // endtime = p.endtime,
                    Inspection_Qty = p.Inspection_Qty,
                    Qualitystage = p.Qualitystage,
                    sampleqty = string.IsNullOrEmpty(p.sampleqty) ? 0 : Convert.ToInt32(p.sampleqty),
                  
                }).ToList();
            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "HoldIndex", "HoldController");
            }
            return View(_List);
        }
        [HttpPost]
        public JsonResult UnHoldAction(string jobNum, string partNum, string qualityStage, string inspectionType, int stageId)
        {
            try
            {
                // 1. Get the stage_part_status from the Stage_Master table
                string stageStatus = DB.Final_Inspection_Stage_Master
                    .Where(l => l.ID == stageId)
                    .Select(l => l.stage_part_status)
                    .FirstOrDefault();

                // 2. Find the Final_Inspection_Data record
                var inspectionData = DB.Final_Inspection_Data.FirstOrDefault(v =>
                    v.JobNum.Trim().ToLower() == jobNum.Trim().ToLower() &&
                    v.PartNum.Trim().ToLower() == partNum.Trim().ToLower() &&
                    v.QualityStage.Trim().ToLower() == qualityStage.Trim().ToLower() &&
                    v.Inspection_Type.Trim().ToLower() == inspectionType.Trim().ToLower());

                if (inspectionData != null)
                {
                    // 3. Update the Hold and Stage status in Final_Inspection_Data
                    if (stageStatus == "12 - Parts in Hold")
                    {
                        inspectionData.Hold = false;
                        inspectionData.Stage = "1 - Parts waiting for Final";
                    }
                    else
                    {
                        inspectionData.Hold = true;
                    }

                    // 4. Find and update matching entry in Final_Inspection_Process
                    var processRecord = DB.Final_Inspection_Process.FirstOrDefault(p =>
                        p.JobNum.Trim().ToLower() == jobNum.Trim().ToLower() &&
                        p.PartNum.Trim().ToLower() == partNum.Trim().ToLower() &&
                        p.Qualitystage.Trim().ToLower() == qualityStage.Trim().ToLower() &&
                        p.Inspection_Type.Trim().ToLower() == inspectionType.Trim().ToLower() &&
                        p.Stage == "20");

                    if (processRecord != null)
                    {
                        processRecord.Stage = "1"; // Update stage from 20 to 1
                    }

                    // 5. Save all changes
                    DB.SaveChanges();

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Record not found" });
            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "UnHoldAction", "HoldController");
                return Json(new { success = false, message = "Internal server error." });
            }
        }




    }
}