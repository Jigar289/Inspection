using Inspection.Web.DataBase;
using Inspection.Web.Models;
using Inspection.Web.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using System.Web.UI.WebControls.WebParts;
using System.Web.Services.Description;
using Microsoft.Ajax.Utilities;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using System.Diagnostics;
using System.Data.Entity.Infrastructure;
using System.Runtime.InteropServices;

namespace Inspection.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        ITEIndiaEntities DB = new ITEIndiaEntities();
        LogService logService = new LogService();
        public ActionResult Index()
        {
            MainhOMEInwardModel maindata = new MainhOMEInwardModel();
            List<HOMEmODEL> finalCounts = new List<HOMEmODEL>();
            HOMEmODEL _modelfinal = new HOMEmODEL();
            HOMEmODEL _modelvisual = new HOMEmODEL();
            HOMEmODEL _modelthared = new HOMEmODEL();
            HOMEmODEL _modelhumidity = new HOMEmODEL();
            try
            {
                var currentDate = DateTime.Now;
                var twoDaysAgo = currentDate.AddDays(-2);
                try
                {
                    List<Final_Inspection_Data> _data = DB.Final_Inspection_Data.Where(v => v.Inward_Date < twoDaysAgo && v.Active == true && v.Delete == false).ToList();

                    if (_data.Count > 0)
                    {
                        maindata._DASHBOARDINWARD = (from model in _data
                                                     where !DB.Final_Inspection_Process.Any(secondEntry => secondEntry.PID == model.ID)
                                                     select new InwardDataModel
                                                     {
                                                         id = model.ID,
                                                         InwardTime = model.Inward_Time,
                                                         InwardDate = model.Inward_Date,
                                                         JobNo = model.JobNum,
                                                         Partno = model.PartNum,
                                                         ProcessStage = model.Stage,
                                                         QualityStage = model.QualityStage,
                                                         ERev = model.EpiRev,
                                                         ActualRev = model.ActRev,
                                                         Qty = model.Inspection_Qty,
                                                         InspectionType = model.Inspection_Type,
                                                     }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    logService.AddLog(ex, "HomeIndex", "HomeController");
                }
                var stages = DB.Final_Inspection_Stage_Master.ToList();

                var finalInspectionData1 = DB.Final_Inspection_Data.Where(k => k.Active == true && k.Delete == false && (k.closerequest == false || k.closerequest == null)).ToList();

                var FINAL = finalInspectionData1.Where(p => p.Inspection_Type == "Final").ToList();
                var VISUAL = finalInspectionData1.Where(p => p.Inspection_Type == "Visual").ToList();
                var THARED = finalInspectionData1.Where(p => p.Inspection_Type == "Thread").ToList();
                var HUMIDITY = finalInspectionData1.Where(p => p.Inspection_Type == "Humidity").ToList();

                Func<List<Final_Inspection_Data>, string, int> calculateCount = (list, stage) =>
                {
                    return list.Where(k => k.Stage == stage).Count();
                };
                try
                {
                    _modelfinal.PartsWaitingForFinalCount = FINAL?.Count(k => k.Stage?.Trim() == "1 - Parts waiting for Final") ?? 0;
                    _modelfinal.PartsWaitingForMRBCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "2 - Parts waiting for MRB") ?? 0;
                    _modelfinal.PartsWaitingForSortingCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "3 - Parts waiting for Sorting" || k.waitingforsorting == true) ?? 0;
                    _modelfinal.PartsWaitingForReworkCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "4 - Parts waiting for Rework" || k.waitingforrework == true) ?? 0;
                    _modelfinal.PartsinreworkCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "8 - Parts in Rework" || k.inrework == true) ?? 0;
                    _modelfinal.ReworkcompleteandwaitingforinspectionCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "6 - Rework complete and waiting for inspection" || k.completeandwaiting == true) ?? 0;
                    _modelfinal.Partsindeviationcount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "7 - Parts in Deviation" || k.indeviation == true) ?? 0;
                    _modelfinal.PartdonothaveunitpriceandrevissueCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "8 - Parts don't have unit price and rev issue") ?? 0;
                    _modelfinal.PartsInspectioncompletedandwaitingforfilecompleteCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "9 - Parts inspection completed and waiting for file complete" || k.completedandwaiting == true) ?? 0;
                    _modelfinal.PartsReadyForpackingCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "10 - Parts Ready For Packing") ?? 0;
                    _modelfinal.PartsmovedfromqualityCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "11 - Parts moved from Quality") ?? 0;
                    _modelfinal.PartsinholdCount = FINAL?.Count(k => k.Stage != null && k.Stage.Trim() == "12 - Parts in Hold") ?? 0;
                    maindata.Final = _modelfinal;

                    maindata.finalpendinginspection = _modelfinal.PartsWaitingForFinalCount + _modelfinal.PartsWaitingForSortingCount + _modelfinal.ReworkcompleteandwaitingforinspectionCount;
                }
                catch (Exception ex)
                {
                    logService.AddLog(ex, "Index", "HomeController");
                }
                try
                {
                    _modelvisual.PartsWaitingForFinalCount = VISUAL?.Count(k => k.Stage.Trim() == "1 - Parts waiting for Visual") ?? 0;
                    _modelvisual.PartsWaitingForMRBCount = VISUAL?.Count(k => k.Stage.Trim() == "2 - Parts waiting for MRB") ?? 0;
                    _modelvisual.PartsWaitingForSortingCount = VISUAL?.Count(k => k.Stage.Trim() == "3 - Parts waiting for Sorting" || k.waitingforsorting == true) ?? 0;
                    _modelvisual.PartsWaitingForReworkCount = VISUAL?.Count(k => k.Stage.Trim() == "4 - Parts waiting for Rework" || k.waitingforrework == true) ?? 0;
                    _modelvisual.PartsinreworkCount = VISUAL?.Count(k => k.Stage.Trim() == "5 - Parts in Rework" || k.inrework == true) ?? 0;
                    _modelvisual.ReworkcompleteandwaitingforinspectionCount = VISUAL?.Count(k => k.Stage.Trim() == "6 - Rework complete and waiting for inspection" || k.completeandwaiting == true) ?? 0;
                    _modelvisual.Partsindeviationcount = VISUAL?.Count(k => k.Stage.Trim() == "7 - Parts in Deviation" || k.indeviation == true) ?? 0;
                    _modelvisual.PartdonothaveunitpriceandrevissueCount = VISUAL?.Count(k => k.Stage.Trim() == "8 - Parts don't have unit price and rev issue") ?? 0;
                    _modelvisual.PartsInspectioncompletedandwaitingforfilecompleteCount = VISUAL?.Count(k => k.Stage.Trim() == "9 - Parts Ready To Next Operation") ?? 0;
                    _modelvisual.PartsReadyForpackingCount = VISUAL?.Count(k => k.Stage.Trim() == "10 - Parts Ready For Packing") ?? 0;
                    _modelvisual.VISUALTHAREDINSPECTIONCOMPLETEDCount = VISUAL?.Count(k => k.Stage.Trim() == "10 - Visual Inspection Completed") ?? 0;
                    _modelvisual.PartsinholdCount = VISUAL?.Count(k => k.Stage.Trim() == "12 - Parts in Hold") ?? 0;
                    maindata.Visual = _modelvisual;

                    maindata.visualpendinginspection = _modelvisual.PartsWaitingForFinalCount + _modelvisual.PartsWaitingForSortingCount + _modelvisual.ReworkcompleteandwaitingforinspectionCount;

                }
                catch (Exception ex)
                {
                    logService.AddLog(ex, "Index", "Home");
                }
                try
                {
                    _modelthared.PartsWaitingForFinalCount = THARED?.Count(k => k.Stage.Trim() == "1 - Parts waiting for Thread") ?? 0;
                    _modelthared.PartsWaitingForMRBCount = THARED?.Count(k => k.Stage.Trim() == "2 - Parts waiting for MRB") ?? 0;
                    _modelthared.PartsWaitingForSortingCount = THARED?.Count(k => k.Stage.Trim() == "3 - Parts waiting for Sorting" || k.waitingforsorting == true) ?? 0;
                    _modelthared.PartsWaitingForReworkCount = THARED?.Count(k => k.Stage.Trim() == "4 - Parts waiting for Rework" || k.waitingforrework == true) ?? 0;
                    _modelthared.PartsinreworkCount = THARED?.Count(k => k.Stage.Trim() == "5 - Parts in Rework" || k.inrework == true) ?? 0;
                    _modelthared.ReworkcompleteandwaitingforinspectionCount = THARED?.Count(k => k.Stage.Trim() == "6 - Rework complete and waiting for inspection" || k.completeandwaiting == true) ?? 0;
                    _modelthared.Partsindeviationcount = THARED?.Count(k => k.Stage.Trim() == "7 - Parts in Deviation" || k.indeviation == true) ?? 0;
                    _modelthared.PartdonothaveunitpriceandrevissueCount = THARED?.Count(k => k.Stage.Trim() == "8 - Parts don't have unit price and rev issue") ?? 0;
                    _modelthared.PartsInspectioncompletedandwaitingforfilecompleteCount = THARED?.Count(k => k.Stage.Trim() == "9 - Parts Ready To Next Operation") ?? 0;
                    _modelthared.PartsReadyForpackingCount = THARED?.Count(k => k.Stage.Trim() == "10 - Parts Ready For Packing") ?? 0;
                    _modelthared.VISUALTHAREDINSPECTIONCOMPLETEDCount = THARED?.Count(k => k.Stage.Trim() == "10 - Thread Inspection Completed") ?? 0;
                    _modelthared.PartsinholdCount = THARED?.Count(k => k.Stage.Trim() == "12 - Parts in Hold") ?? 0;
                    maindata.Thread = _modelthared;

                    maindata.tharedpendinginspection = _modelthared.PartsWaitingForFinalCount + _modelthared.PartsWaitingForSortingCount + _modelthared.ReworkcompleteandwaitingforinspectionCount;

                }
                catch (Exception ex)
                {
                    logService.AddLog(ex, "Index", "Home");
                }
                try
                {
                    _modelhumidity.PartsWaitingForFinalCount = HUMIDITY?.Count(k => k.Stage.Trim() == "1 - Parts waiting for Humidity") ?? 0;
                    _modelhumidity.PartsWaitingForMRBCount = HUMIDITY?.Count(k => k.Stage.Trim() == "2 - Parts waiting for MRB") ?? 0;
                    _modelhumidity.PartsWaitingForSortingCount = HUMIDITY?.Count(k => k.Stage.Trim() == "3 - Parts waiting for Sorting" || k.waitingforsorting == true) ?? 0;
                    _modelhumidity.PartsWaitingForReworkCount = HUMIDITY?.Count(k => k.Stage.Trim() == "4 - Parts waiting for Rework" || k.waitingforrework == true) ?? 0;
                    _modelhumidity.PartsinreworkCount = HUMIDITY?.Count(k => k.Stage.Trim() == "8 - Parts in Rework" || k.inrework == true) ?? 0;
                    _modelhumidity.ReworkcompleteandwaitingforinspectionCount = HUMIDITY?.Count(k => k.Stage.Trim() == "6 - Rework complete and waiting for inspection" || k.completeandwaiting == true) ?? 0;
                    _modelhumidity.Partsindeviationcount = HUMIDITY?.Count(k => k.Stage.Trim() == "7 - Parts in Deviation" || k.indeviation == true) ?? 0;
                    _modelhumidity.PartdonothaveunitpriceandrevissueCount = HUMIDITY?.Count(k => k.Stage.Trim() == "8 - Parts don't have unit price and rev issue") ?? 0;
                    _modelhumidity.PartsInspectioncompletedandwaitingforfilecompleteCount = HUMIDITY?.Count(k => k.Stage.Trim() == "9 - Parts inspection completed and waiting for file complete") ?? 0;
                    _modelhumidity.PartsReadyForpackingCount = HUMIDITY?.Count(k => k.Stage.Trim() == "10 - Parts Ready For Packing") ?? 0;
                    _modelhumidity.PartsmovedfromqualityCount = HUMIDITY?.Count(k => k.Stage.Trim() == "14 - Parts moved from Quality") ?? 0;
                    _modelhumidity.PartsinholdCount = HUMIDITY?.Count(k => k.Stage.Trim() == "12 - Parts in Hold") ?? 0;
                    maindata.Humidity = _modelhumidity;

                    maindata.humiditypendinginspection = _modelhumidity.PartsWaitingForHumidityCount + _modelhumidity.PartsWaitingForSortingCount + _modelhumidity.ReworkcompleteandwaitingforinspectionCount;

                }
                catch (Exception EX)
                {
                    logService.AddLog(EX, "Index", "Home");
                }
            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "HomeIndex", "HomeController");
            }
            return View(maindata);

        }
        public ActionResult Getdatatables(string Type, string stage)
        {
            List<InwardDataModel> _model = new List<InwardDataModel>();
            List<Final_Inspection_Stage_Data> Data = new List<Final_Inspection_Stage_Data>();
            Decisionmodel _Dmodel = new Decisionmodel();
            reworkmodel reworkmodel = new reworkmodel();
            Submodel submodel = new Submodel();
            List<Submodel> _Slist = new List<Submodel>();
            AddDecisionmodel _Mainmodel = new AddDecisionmodel();

            int stgeno = Convert.ToInt32(stage);
            try
            {
                string _Stages = DB.Final_Inspection_Stage_Master.Where(l => l.Stage == stgeno).Select(l => l.stage_part_status).FirstOrDefault();
                if (_Stages.Trim() == "1 - Parts waiting for Final")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && p.Stage.Trim() == _Stages && (p.closerequest == false || p.closerequest == null))
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  currentstage = stage,
                                  sortingqty = modal.Sorting_Qty,
                                  deviationqty = modal.Deviation_Qty,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();

                }
                if (_Stages.Trim() == "1 - Parts waiting for Thread")
                {

                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && p.Stage.Trim() == _Stages && (p.closerequest == false || p.closerequest == null))
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  currentstage = stage,
                                  sortingqty = modal.Sorting_Qty,
                                  deviationqty = modal.Deviation_Qty,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                if (_Stages.Trim() == "1 - Parts waiting for Visual")
                {

                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && p.Stage.Trim() == _Stages && (p.closerequest == false || p.closerequest == null))
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  currentstage = stage,
                                  sortingqty = modal.Sorting_Qty,
                                  deviationqty = modal.Deviation_Qty,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                if (_Stages.Trim() == "1 - Parts waiting for Humidity")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && p.Stage.Trim() == _Stages && (p.closerequest == false || p.closerequest == null))
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  currentstage = stage,
                                  sortingqty = modal.Sorting_Qty,
                                  deviationqty = modal.Deviation_Qty,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "3 - Parts waiting for Sorting")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.waitingforsorting == true) && (p.closerequest == false || p.closerequest == null) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  currentstage = stage,
                                  sortingqty = modal.Sorting_Qty,
                                  deviationqty = modal.Deviation_Qty,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();

                }
                else if (_Stages.Trim() == "4 - Parts waiting for Rework")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.waitingforrework == true) && (p.closerequest == false || p.closerequest == null) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  currentstage = stage,
                                  reworkqty = modal.Rework_Qty,
                                  deviationqty = modal.Deviation_Qty,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "5 - Parts in Rework")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.inrework == true) && (p.closerequest == false || p.closerequest == null) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  reworkqty = modal.Rework_Qty,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "6 - Rework complete and waiting for inspection")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.completeandwaiting == true) && (p.closerequest == false || p.closerequest == null))
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  reworkqty = modal.Rework_Qty,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "7 - Parts in Deviation")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.indeviation == true) && (p.closerequest == false || p.closerequest == null) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "8 - Parts don't have unit price and rev issue")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.indeviation == true) && (p.closerequest == false || p.closerequest == null) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "9 - Parts inspection completed and waiting for file complete")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.completedandwaiting == true) && (p.closerequest == false || p.closerequest == null) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "9 - Parts Ready To Next Operation")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim()) && (p.closerequest == false || p.closerequest == null) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "11 - Parts moved from Quality")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.movedfromquality == true) && (p.closerequest == false || p.closerequest == null))
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "10 - Visual Inspection Completed")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.movedfromquality == true) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "10 - Thread Inspection Completed")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.movedfromquality == true) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "10 - Thread Inspection Completed")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim() || p.movedfromquality == true) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "10 - Parts Ready For Packing")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim()) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                else if (_Stages.Trim() == "12 - Parts in Hold")
                {
                    _model = (from modal in DB.Final_Inspection_Data.Where(p => p.Active == true && p.Delete == false && p.Inspection_Type == Type && (p.Stage.Trim() == _Stages.Trim()) && p.Active == true && p.Delete == false)
                              select new InwardDataModel
                              {
                                  id = modal.ID,
                                  InwardDate = modal.Inward_Date,
                                  Qty = modal.Inspection_Qty,
                                  InwardTime = modal.Inward_Time,
                                  JobNo = modal.JobNum,
                                  Partno = modal.PartNum,
                                  ProcessStage = modal.Stage,
                                  ERev = modal.EpiRev,
                                  ActualRev = modal.ActRev,
                                  InspectionType = modal.Inspection_Type,
                                  deviationqty = modal.Deviation_Qty,
                                  currentstage = stage,
                                  Note = modal.Note,
                                  QualityStage = modal.QualityStage
                              }).ToList();
                }
                if (_Stages.Trim() == "2 - Parts waiting for MRB")
                {
                    return RedirectToAction("Index", "MRB", new { inspectiotype = Type });
                }
            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "Getdatatables", "Home");
            }
            var model = new AddDecisionmodel
            {
                _INWARDList = _model,
                _submodeldata = _Dmodel,
                _Mainreworkmodel = new reworkmodel(),

            };
            return PartialView("Getdatatables", model);
        }
        [Authorize]
        public ActionResult PartHistory(string jobnum, int id, string type, string stage)
        {

            MainInwardModel inwardData = new MainInwardModel();

            inwardData._INWARD = (from model in DB.Final_Inspection_Data.Where(v => v.JobNum == jobnum && v.QualityStage == stage && v.Inspection_Type == type && v.Active == true && v.Delete == false)
                                  select new InwardDataModel
                                  {
                                      id = model.ID,
                                      InwardTime = model.Inward_Time,
                                      InwardDate = model.Inward_Date,
                                      JobNo = model.JobNum,
                                      Partno = model.PartNum,
                                      ProcessStage = model.Stage,
                                      ERev = model.EpiRev,
                                      ActualRev = model.ActRev,
                                      Qty = model.Inspection_Qty,
                                      InspectionType = model.Inspection_Type,
                                      Statuschange = model.Statuschange,
                                      SampleQuantity = model.Sample_Qty,
                                  }).FirstOrDefault();

            int ID = inwardData._INWARD.id;
            string _ID = inwardData._INWARD.id.ToString();

            inwardData._INWARDListdata = (from model in DB.Final_Inspection_Stage_Data.Where(v => v.Inspection_ID == _ID && v.Active == true && v.Deleted == false)
                                          select new InwardDataModel
                                          {
                                              id = model.ID,
                                              InwardTime = model.CurrentDateTime,
                                              JobNo = model.JobNum,
                                              Partno = model.PartNum,
                                              ProcessStage = model.Stage,
                                              InspectionType = model.InspectionType,
                                          }).ToList();

            return View(inwardData);
        }
        public ActionResult Stagechange(string selectedValue, string id, string qty)
        {
            string successMessage = "";
            string warningMessage = "";
            string errormessage = "";

            Final_Inspection_Stage_Data _stage = new Final_Inspection_Stage_Data();
            try
            {
                if (selectedValue != null && selectedValue != "undefined" && id != null)
                {
                    string[] parts = selectedValue.Split(new string[] { " - " }, StringSplitOptions.None);
                    int? _stages = Convert.ToInt32(parts[0].Trim());

                    string _Stage = DB.Final_Inspection_Stage_Master.Where(l => l.Stage == _stages).Select(l => l.stage_part_status).FirstOrDefault();

                    int _id = Convert.ToInt32(id);
                    Final_Inspection_Data _data = DB.Final_Inspection_Data.Where(p => p.ID == _id).FirstOrDefault();
                    if (_data != null)
                    {

                        string stag = DB.Final_Inspection_Stage_Master.Where(l => l.stage_part_status == _data.Stage).Select(l => l.stage_part_status).FirstOrDefault();
                        _stage.Inspection_ID = _data.ID.ToString();
                        _stage.MID = _data.MID;
                        _stage.JobNum = _data.JobNum;
                        _stage.PartNum = _data.PartNum;
                        _stage.stageno = _Stage.ToString();
                        _stage.Stage = _data.Stage;
                        _stage.InspectionType = _data.Inspection_Type;
                        _stage.Qty = _data.Inspection_Qty;
                        _stage.Active = true;
                        _stage.Deleted = false;
                        _stage.CurrentDateTime = DateTime.Now.ToString();
                        DB.Final_Inspection_Stage_Data.Add(_stage);

                        _data.Stage = _Stage;
                        _data.waitingforsorting = false;
                        //_data.Inspection_Qty = selectedValue;
                        DB.SaveChanges();

                    }
                    successMessage = "Status Changed Successfually!";
                }
            }
            catch (Exception)
            {
                warningMessage = "Status Changed Successfually!";
            }
            var response = new
            {

                successMessage = successMessage,
                errormessage = errormessage,
                warningMessage = warningMessage,

            };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Addsorting(AddDecisionmodel _model)
        {
            MainInwardModel inwardData = new MainInwardModel();
            Final_Inspection_Stage_Data _stage = new Final_Inspection_Stage_Data();
            Final_Inspection_Data _data = new Final_Inspection_Data();
            string stag = "";
            int? _stages = null;
            try
            {
                Final_Inspection_MRB_DecisionData _Data = new Final_Inspection_MRB_DecisionData();
                if (_model != null)
                {
                    _Data.IID = _model._submodeldata.id;
                    _Data.MRBDecision = _model._submodeldata.InspectionTYPE;
                    _Data.Date = _model._submodeldata.StartDate;
                    _Data.Time = _model._submodeldata.StartTime;
                    _Data.OkQty = _model._submodeldata.OkQty;

                    _Data.RejectQty = _model._submodeldata.RejectQty != null ? _model._submodeldata.RejectQty : "";
                    _Data.Remark = _model._submodeldata.Remark != null ? _model._submodeldata.Remark : "";
                    _Data.Active = true;
                    _Data.Deleted = false;
                    DB.Final_Inspection_MRB_DecisionData.Add(_Data);
                    DB.SaveChanges();
                    _data = DB.Final_Inspection_Data.Where(x => x.ID == _model._submodeldata.id).FirstOrDefault();
                    if (_data != null)
                    {
                        stag = DB.Final_Inspection_Stage_Master.Where(l => l.stage_part_status == _data.Stage).Select(l => l.stage_part_status).FirstOrDefault();
                        _stage.Inspection_ID = _data.ID.ToString();
                        _stage.MID = _data.MID;
                        _stage.JobNum = _data.JobNum;
                        _stage.PartNum = _data.PartNum;
                        _stage.stageno = _model._submodeldata.stage;
                        _stage.Stage = _data.Stage;
                        _stage.InspectionType = _data.Inspection_Type;
                        _stage.Qty = _data.Inspection_Qty;
                        _stage.Active = true;
                        _stage.Deleted = false;
                        _stage.CurrentDateTime = DateTime.Now.ToString();
                        DB.Final_Inspection_Stage_Data.Add(_stage);
                        DB.SaveChanges();
                        if (_model._submodeldata.InspectionTYPE == "Sorting" || _model._submodeldata.TYPE == "Sorting")
                        {
                            TimeSpan? sortingTimeSpan = _data.InSortingDate - _model._submodeldata.StartDate;
                            _data.Sortingtime = sortingTimeSpan.HasValue ? (decimal?)sortingTimeSpan.Value.TotalHours : (decimal?)0;
                        }
                        

                        if (_model._submodeldata.InspectionTYPE == "Rework" || _model._submodeldata.TYPE == "Rework")
                        {
                            int? rjqty = 0;
                            int? okqty = 0;
                            if (_model._submodeldata.RejectQty != null && _model._submodeldata.RejectQty != "0")
                            {
                                rjqty = Convert.ToInt32(_model._submodeldata.RejectQty);
                            }
                            if (_model._submodeldata.OkQty != null)
                            {
                                okqty = Convert.ToInt32(_model._submodeldata.OkQty);
                            }
                            _data.inrework = false;
                            _data.completeandwaiting = true;

                            _data.Reject_Qty = rjqty;
                            _data.Rework_Qty = okqty;

                            TimeSpan? reworkTimeSpan = _data.InReworkDate - _model._submodeldata.StartDate;
                            _data.Reworktime = reworkTimeSpan.HasValue ? (decimal)reworkTimeSpan.Value.TotalHours : (decimal)0;
                        }

                        if (_model._submodeldata.stage != null)
                        {
                            _data.Stage = _model._submodeldata.stage;
                            if (_model._submodeldata.InspectionTYPE == "Sorting")
                            {
                                _data.waitingforsorting = false;
                            }
                            else if (_model._submodeldata.InspectionTYPE == "Rework")
                            {
                                _data.inrework = false;
                            }
                            else if (_model._submodeldata.InspectionTYPE == "Daviation")
                            {
                                _data.indeviation = false;
                            }
                        }
                        else
                        {
                            if (_model._submodeldata.TYPE == "Rework")
                            {
                                stag = "9 - Rework complete and waiting for inspection";

                                _data.inrework = false;
                                _data.Stage = "6 - Rework complete and waiting for inspection";
                            }
                        }
                        //TimeSpan? reworkTimeSpan = _data.InReworkDate - _model._submodeldata.StartDate;
                        //_data.Reworktime = reworkTimeSpan.HasValue ? (decimal?)reworkTimeSpan.Value.TotalHours : (decimal?)0;
                        DB.SaveChanges();
                    }
                }
                string[] parts = stag.Split(new string[] { " - " }, StringSplitOptions.None);
                _stages = Convert.ToInt32(parts[0].Trim());
            }
            catch (DbEntityValidationException ex)
            {
                //errormessage = ex.Message;

                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    logService.AddLog(ex, " b", "Home");
            //}
            return RedirectToAction("Getdatatables", new { Type = _data.Inspection_Type, stage = _stages });
        }
        public ActionResult CloseInspection(int id, string jobno, string stage)
        {
            MainInwardModel inwardData = new MainInwardModel();

            string successMessage = "";
            string warningMessage = "";
            string errormessage = "";
            try
            {
                List<Final_Inspection_Data> _Inspection_Data = DB.Final_Inspection_Data
                    .Where(v => v.JobNum == jobno && (v.QualityStage == stage || (v.QualityStage.Contains(stage))) && (v.closerequest == false || v.closerequest == null) && v.Active == true && v.Delete == false)
                    .ToList();

                bool hasHold = _Inspection_Data.Any(v => v.Hold == true);
                if (!hasHold)
                {
                    if (_Inspection_Data.Any())
                    {
                        int? acceptQty = 0;
                        int rejectSum = 0;
                        int reworkSum = 0;
                        int soertingSum = 0;
                        int resortingSum = 0;
                        int daviationSum = 0;
                        int reworkmrbSum = 0;
                        int remesuredSum = 0;
                        int splitSum = 0;
                        int holdSum = 0;
                        try
                        {
                            bool jobExists = DB.Final_Inspection_Mrb_Data.Any(first => first.JobNo == jobno && (first.QualityStage == stage || first.QualityStage.Contains(stage))); //
                            if (jobExists)
                            {
                                var totals = DB.Final_Inspection_Mrb_Data
                                    .Where(first => first.JobNo == jobno && first.QualityStage == stage)
                                    .Join(
                                        DB.Final_Inspection_Mrb_Rcode,
                                        first => first.ID,
                                        second => second.PID,
                                        (first, second) => new
                                        {
                                            second.Reject,
                                            second.Rework,
                                            second.Sorting,
                                            second.Resorting,
                                            second.Deviation,
                                            second.Reworkinmrb,
                                            second.Remeasured,
                                            second.Split,
                                            second.Hold
                                        })
                                    .GroupBy(x => 1) // Group all results to calculate aggregate in one go
                                    .Select(g => new
                                    {
                                        TotalReject = g.Sum(x => (int?)x.Reject ?? 0),
                                        TotalRework = g.Sum(x => (int?)x.Rework ?? 0),
                                        TotalSorting = g.Sum(x => (int?)x.Sorting ?? 0),
                                        TotalResorting = g.Sum(x => (int?)x.Resorting ?? 0),
                                        TotalDeviation = g.Sum(x => (int?)x.Deviation ?? 0),
                                        TotalReworkMrb = g.Sum(x => (int?)x.Reworkinmrb ?? 0),
                                        TotalRemeasured = g.Sum(x => (int?)x.Remeasured ?? 0),
                                        TotalSplit = g.Sum(x => (int?)x.Split ?? 0),
                                        TotalHold = g.Sum(x => (int?)x.Hold ?? 0)
                                    }).FirstOrDefault();

                                //Check if totals were found; set to 0 if not.
                                rejectSum = totals?.TotalReject ?? 0;
                                reworkSum = totals?.TotalRework ?? 0;
                                soertingSum = totals?.TotalSorting ?? 0;
                                resortingSum = totals?.TotalResorting ?? 0;
                                daviationSum = totals?.TotalDeviation ?? 0;
                                reworkmrbSum = totals?.TotalReworkMrb ?? 0;
                                remesuredSum = totals?.TotalRemeasured ?? 0;
                                splitSum = totals?.TotalSplit ?? 0;
                                holdSum = totals?.TotalHold ?? 0;
                                if (reworkSum > 0)
                                {
                                    if (_Inspection_Data.Any(p => p.inrework == true) || _Inspection_Data.Any(p => p.waitingforrework == true) || _Inspection_Data.Any(p => p.completeandwaiting == true))
                                    {
                                        warningMessage += $"This job requires {reworkSum} rework(s).\n";
                                    }
                                }
                                if (soertingSum > 0)
                                {
                                    if (_Inspection_Data.Any(p => p.waitingforsorting == true))
                                    {
                                        warningMessage += $"This job requires {soertingSum} sorting(s).\n";
                                    }
                                }
                                if (resortingSum > 0)
                                {
                                    if (_Inspection_Data.Any(p => p.waitingforsorting == true))
                                    {
                                        warningMessage += $"This job requires {resortingSum} resorting(s).\n";
                                    }
                                }
                                if (daviationSum > 0)
                                {
                                    if (_Inspection_Data.Any(p => p.indeviation == true))
                                    {
                                        warningMessage += $"This job has {daviationSum} deviation(s).\n";
                                    }
                                }
                                if (reworkmrbSum > 0)
                                {
                                    warningMessage += $"This job has {reworkmrbSum} rework(s) in MRB.\n";
                                }
                                if (remesuredSum > 0)
                                {
                                    warningMessage += $"This job has {remesuredSum} remeasured(s).\n";
                                }
                                if (splitSum > 0)
                                {
                                    if (_Inspection_Data.Any(p => p.split == true))
                                    {
                                        warningMessage += $"This job requires {splitSum} split(s).\n";
                                    }
                                }
                                if (holdSum > 0)
                                {
                                    if (_Inspection_Data.Any(p => p.Hold == true))
                                    {
                                        warningMessage += $"This job has {holdSum} hold(s).\n";
                                    }
                                }
                            }
                            else
                            {
                                rejectSum = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        string qty = DB.Final_Inspection_Data.Where(p => p.JobNum == jobno && (p.QualityStage == stage || p.QualityStage.Contains(stage)) && p.Inspection_Type == "Final").Select(p => p.Inspection_Qty).FirstOrDefault();

                        int _qty = Convert.ToInt32(qty);

                        int _allqty = _qty - rejectSum;

                        acceptQty = _qty - rejectSum - reworkSum - soertingSum - resortingSum - daviationSum - reworkmrbSum - remesuredSum - splitSum - holdSum;

                        if (string.IsNullOrEmpty(warningMessage))
                        {
                            _Inspection_Data.ForEach(item => item.closerequest = true);
                            DB.SaveChanges();

                            List<Final_Inspection_Data> final_Inspection_Data = DB.Final_Inspection_Data.Where(p => p.JobNum == jobno && (p.QualityStage == stage || p.QualityStage.Contains(stage)) && p.Active == true && p.Delete == false && p.closerequest == true).ToList();

                            if (final_Inspection_Data != null)
                            {
                                foreach (var item in final_Inspection_Data)
                                {
                                    int inwardqty = Convert.ToInt32(item.Inspection_Qty);

                                    // Percentages 
                                    decimal rejectionPercentage = ((decimal)(item.Reject_Qty ?? 0) / inwardqty) * 100;
                                    decimal reworkper = ((decimal)(item.Rework_Qty ?? 0) / inwardqty) * 100;
                                    decimal daviationper = ((decimal)(item.Deviation_Qty ?? 0) / inwardqty) * 100;

                                    // Lot Reject
                                    string lotReject = (item.Reject_Qty ?? 0) > 0 ? "Yes" : "No";

                                    // Inward Time Calculation

                                    TimeSpan? timeDifference = item.Inward_Date - item.CloseRequstDate;
                                    decimal? inwardtime = timeDifference.HasValue ? (decimal)timeDifference.Value.TotalHours : 0;
                                    decimal? totalhr = item.Reworktime.HasValue ? inwardtime - item.Reworktime : inwardtime;
                                    TimeSpan? MRBTakentime = item.Mrb_Create_date - item.MRBDate;


                                    TimeSpan? finalDuration = TimeSpan.Zero;
                                    TimeSpan reworkDuration = TimeSpan.Zero;
                                    TimeSpan sortingDuration = TimeSpan.Zero;
                                    TimeSpan finalInspectorTotalTime = TimeSpan.Zero;
                                    TimeSpan? visualDuration = TimeSpan.Zero;
                                    TimeSpan? threadDuration = TimeSpan.Zero;
                                    TimeSpan? humidityDuration = TimeSpan.Zero;
                                    TimeSpan? totalInspectionTime = TimeSpan.Zero;

                                    foreach (var process in final_Inspection_Data)
                                    {
                                        TimeSpan? processDuration = item.Inward_Date - item.CloseRequstDate;
                                        switch (process.Inspection_Type)
                                        {
                                            case "Final":
                                                finalDuration += processDuration;
                                                break;
                                            case "Visual":
                                                visualDuration += processDuration;
                                                break;
                                            case "Thread":
                                                threadDuration += processDuration;
                                                break;
                                            case "Humidity":
                                                humidityDuration += processDuration;
                                                break;
                                        }
                                    }

                                    List<Final_Inspection_Process> _Process = DB.Final_Inspection_Process.Where(p => p.PID == item.ID).ToList();
                                    if (_Process != null)
                                    {
                                        foreach (var process in _Process)
                                        {
                                            // Ensure both Inspection_date and endtime exist
                                            if (process.Inspection_date.HasValue && process.endtime.HasValue)
                                            {
                                                // Add the difference (endtime - Inspection_date) to the total time
                                                finalInspectorTotalTime += process.endtime.Value - process.Inspection_date.Value;
                                            }
                                        }

                                        List<Final_Inspection_MRB_DecisionData> _RSDTIME = DB.Final_Inspection_MRB_DecisionData.Where(P => P.Active == true && P.Deleted == false && P.IID == item.ID).ToList();

                                        foreach (var items in _RSDTIME)
                                        {
                                            // Calculate durations based on inspection type and MRB decision
                                            if (items.MRBDecision == "Final" || items.MRBDecision == "Visual" || items.MRBDecision == "Thared" || items.MRBDecision == "Humidity") // final means rework
                                            {
                                                reworkDuration += (items.Date - item.InReworkDate) ?? TimeSpan.Zero;
                                            }
                                            if (items.MRBDecision == "Sorting")
                                            {
                                                sortingDuration += (items.Date - item.InSortingDate) ?? TimeSpan.Zero;
                                            }
                                            if (items.MRBDecision == "Daviation")
                                            {
                                                sortingDuration += (items.Date - item.InDaviationTime) ?? TimeSpan.Zero;
                                            }
                                        }
                                        //Aggregate total time
                                        TimeSpan? InspectiontimeManpowerspendtime = timeDifference + reworkDuration + sortingDuration + finalInspectorTotalTime;
                                        TimeSpan? TotalinspectiontimeminusManpowerspendtime = finalDuration + visualDuration + threadDuration + humidityDuration + reworkDuration + sortingDuration;
                                        TimeSpan? InspectiontimecommonQualitydivisionspendtimebypart = timeDifference + reworkDuration + sortingDuration; // comaan inspectiontime add
                                        TimeSpan? TotalinspectiontimeCommonqualitydivisionspendtimebypart = finalDuration + visualDuration + threadDuration + humidityDuration + reworkDuration + sortingDuration; // coman inspection time


                                        TimeSpan Reworktime = reworkDuration;
                                        TimeSpan Sortingtime = sortingDuration;
                                        TimeSpan? daviationwaitingtime = sortingDuration;

                                        item.rejectpersentage = rejectionPercentage;
                                        item.Reworkpersentage = reworkper;
                                        item.Deviationpersentage = daviationper;
                                        item.lotreject = lotReject;
                                        item.TotalTimeinquality = totalhr;
                                        item.InspectiontimeManpowerspendtime = InspectiontimeManpowerspendtime.ToString();
                                        item.TotalinspectiontimeManpowerspendtime = TotalinspectiontimeminusManpowerspendtime.ToString();
                                        item.InspectiontimeQualitydivisionspendtimebypart = InspectiontimecommonQualitydivisionspendtimebypart.ToString();
                                        item.Totalinspectiontimequalitydivisionspendtimebypart = TotalinspectiontimeCommonqualitydivisionspendtimebypart.ToString();
                                        item.MRBTakentime = MRBTakentime.ToString();

                                    }
                                    DB.SaveChanges();
                                }
                            }
                            successMessage = "Process Ended Successfully";
                        }
                        else
                        {
                            warningMessage = warningMessage;
                        }
                    }
                }
                else
                {
                    warningMessage = "Part In Hold so not close this request";
                }
                
            }
            catch (Exception EX)
            {
                logService.AddLog(EX, "CloseInspection", "Home");
                errormessage = "An error occurred while processing your request.";
            }
            var response = new
            {
                successMessage = successMessage,
                errormessage = errormessage,
                warningMessage = warningMessage
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult INReworkData(string check, int id, string instype)
        {

            MainInwardModel inwardData = new MainInwardModel();
            string successMessage = "";
            string warningMessage = "";
            string errormessage = "";
            try
            {
                if (instype == "Packing")
                {
                    Final_Inspection_Data _Inspection_Data = DB.Final_Inspection_Data.Where(v => v.ID == id && v.Active == true && v.Delete == false).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
                        if (check == "1")
                        {
                            _Inspection_Data.movedfromquality = true;
                            _Inspection_Data.readyforpacking = false;
                            _Inspection_Data.Stage = "11 - Parts moved from Quality";
                        }
                        successMessage = "Woow ..Move to in Last Stage.";
                    }
                }
                else
                {
                    Final_Inspection_Data _Inspection_Data = DB.Final_Inspection_Data.Where(v => v.ID == id && v.Active == true && v.Delete == false).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
                        if (check == "1")
                        {
                            _Inspection_Data.waitingforrework = false;
                            _Inspection_Data.inrework = true;
                            _Inspection_Data.Stage = "5 - Parts in Rework";
                        }
                    }
                    successMessage = "Woow ..Move to in rework.";
                }
                DB.SaveChanges();
            }
            catch (Exception EX)
            {
                logService.AddLog(EX, "CloseInspection", "Home");
            }
            var response = new
            {
                successMessage = successMessage,
                errormessage = errormessage,
                warningMessage = warningMessage,

            };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult stageChangedata(string check, int id, string instype)
        {

            MainInwardModel inwardData = new MainInwardModel();
            string successMessage = "";
            string warningMessage = "";
            string errormessage = "";
            try
            {
                if (instype == "redynextoperation")
                {
                    Final_Inspection_Data _Inspection_Data = DB.Final_Inspection_Data.Where(v => v.ID == id && v.Active == true && v.Delete == false).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
                        if (_Inspection_Data.Inspection_Type == "Visual")
                        {
                            _Inspection_Data.visualcompleted = true;
                            _Inspection_Data.completedandwaiting = false;
                            _Inspection_Data.Stage = "10 - Visual Inspection Completed";
                        }
                        else
                        {
                            _Inspection_Data.threadcompleted = true;
                            _Inspection_Data.completedandwaiting = false;
                            _Inspection_Data.Stage = "10 - Thread Inspection Completed";
                        }
                    }

                }
                DB.SaveChanges();
                successMessage = "Woow ..Move to in Last Stage.";
            }
            catch (Exception EX)
            {
                logService.AddLog(EX, "CloseInspection", "Home");
            }
            var response = new
            {
                successMessage = successMessage,
                errormessage = errormessage,
                warningMessage = warningMessage,

            };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult reworkpagePage(int id)
        {
            InwardDataModel List = new InwardDataModel();
            List<InwardDataModel> _data = new List<InwardDataModel>();
            List<Submodel> _List = new List<Submodel>();
            Submodel _submodel = new Submodel();

            string ids = id.ToString();

            int? totalfinalInspectedQty = null;
            int? totalvisualInspectedQty = null;
            int? totaltharedInspectedQty = null;
            bool? finalsts = false;
            bool? visualsts = false;
            bool? thareadsts = false;
            bool? fhumiditysts = false;
            bool? reworkstatuschange = null;

            try
            {
                int finalqty = 0;
                var visualqty = "";
                var thareadqty = "";
                var fhumidityqty = "";

                List = (from model in DB.Final_Inspection_Data.Where(p => p.ID == id && p.Active == true && p.Delete == false)
                        select new InwardDataModel
                        {
                            id = model.ID,
                            InwardTime = model.Inward_Time,
                            InwardDate = model.Inward_Date,
                            JobNo = model.JobNum,
                            Partno = model.PartNum,
                            ProcessStage = model.Stage,
                            ERev = model.EpiRev,
                            ActualRev = model.ActRev,
                            Qty = model.Inspection_Qty,
                            InspectionType = model.Inspection_Type,
                            Statuschange = model.Statuschange,
                            SampleQuantity = model.Sample_Qty,
                            reworkqty = model.Rework_Qty,
                            //acceptqty = acceptQty,
                            Note = model.Note,
                            //currentstage = value,
                            finalinspection = finalqty,
                            visualinspection = visualqty,
                            threadinspection = thareadqty,
                            humidity = fhumidityqty,
                        }).FirstOrDefault();

                _List = (from model in DB.Final_Inspection_Process_Rework.Where(p => p.Inspection_ID == ids && p.Inspection_Type == List.InspectionType && p.DecisionType == "Rework")
                         select new Submodel
                         {
                             id = model.ID,
                             inspectiondate = model.Inspection_date,
                             EndTime = model.endtime,
                             InspectedQty = model.Inspectionqty,
                             InspectionBy = model.done_by,
                             InspectionTYPE = model.Inspection_Type,
                         }).ToList();

                var reworkData = DB.Final_Inspection_Process_Rework
     .Where(p => p.Inspection_ID == ids && p.Inspection_Type == List.InspectionType && p.DecisionType == "Rework")
     .Select(p => new { p.Inspectionqty, p.reworkstatuschange })
     .ToList();

                totalfinalInspectedQty = reworkData.Sum(p => p.Inspectionqty);
                reworkstatuschange = reworkData.Select(p => p.reworkstatuschange).FirstOrDefault();
            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "reworkpagePage", "Home");
            }
            var maininwarddata = new mAINPROGRESSModel
            {
                _INWARD = List,
                SUBMODEL = _List,
                TOTALreworkQTY = totalfinalInspectedQty,
                finalstatus = finalsts,
                visualstatus = visualsts,
                tharedstatus = thareadsts,
                humiditystatus = fhumiditysts,
                _INWARDList = _data,
                _submodel = _submodel,
                Reworkstatus = reworkstatuschange,

            };
            return PartialView("_AddRework", maininwarddata);
        }
        public ActionResult SortingPage(int id)
        {
            InwardDataModel List = new InwardDataModel();
            List<InwardDataModel> _data = new List<InwardDataModel>();
            List<Submodel> _List = new List<Submodel>();
            Submodel _submodel = new Submodel();

            string ids = id.ToString();

            int? totalfinalInspectedQty = null;
            int? totalvisualInspectedQty = null;
            int? totaltharedInspectedQty = null;
            bool? finalsts = false;
            bool? visualsts = false;
            bool? thareadsts = false;
            bool? fhumiditysts = false;
            int? totalhumidityInspectedQty = null;

            try
            {
                int finalqty = 0;
                var visualqty = "";
                var thareadqty = "";
                var fhumidityqty = "";

                List = (from model in DB.Final_Inspection_Data.Where(p => p.ID == id)
                        select new InwardDataModel
                        {
                            id = model.ID,
                            InwardTime = model.Inward_Time,
                            InwardDate = model.Inward_Date,
                            JobNo = model.JobNum,
                            Partno = model.PartNum,
                            ProcessStage = model.Stage,
                            ERev = model.EpiRev,
                            ActualRev = model.ActRev,
                            Qty = model.Inspection_Qty,
                            InspectionType = model.Inspection_Type,
                            Statuschange = model.Statuschange,
                            SampleQuantity = model.Sample_Qty,
                            deviationqty = model.Deviation_Qty,
                            sortingqty = model.Sorting_Qty,
                            Note = model.Note,
                            //currentstage = value,
                            finalinspection = finalqty,
                            visualinspection = visualqty,
                            threadinspection = thareadqty,
                            humidity = fhumidityqty,
                        }).FirstOrDefault();

                _List = (from model in DB.Final_Inspection_Process_Rework.Where(p => p.Inspection_ID == ids && p.Inspection_Type == List.InspectionType && p.DecisionType == "Sorting")
                         select new Submodel
                         {
                             id = model.ID,
                             inspectiondate = model.Inspection_date,
                             EndTime = model.endtime,
                             InspectedQty = model.Inspectionqty,
                             InspectionBy = model.done_by,
                             InspectionTYPE = model.Inspection_Type,
                         }).ToList();

                totalfinalInspectedQty = DB.Final_Inspection_Process_Rework.Where(p => p.Inspection_ID == ids && p.Inspection_Type == List.InspectionType && p.DecisionType == "Sorting").Sum(p => p.Inspectionqty);

            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "SortingPage", "Home");
            }
            var maininwarddata = new mAINPROGRESSModel
            {
                _INWARD = List,
                SUBMODEL = _List,
                TOTALreworkQTY = totalfinalInspectedQty,
                finalstatus = finalsts,
                visualstatus = visualsts,
                tharedstatus = thareadsts,
                humiditystatus = fhumiditysts,
                _INWARDList = _data,
                _submodel = _submodel,
            };
            return PartialView("_AddSorting", maininwarddata);
        }
        public ActionResult AddReworkDataData(mAINPROGRESSModel _model)
        {

            MainInwardModel inwardData = new MainInwardModel();
            Final_Inspection_Stage_Data _stage = new Final_Inspection_Stage_Data();
            Final_Inspection_Data _data = new Final_Inspection_Data();
            int sid = 0;
            int? _stages = null;
            try
            {
                Final_Inspection_Process_Rework _Data = new Final_Inspection_Process_Rework();
                if (_model != null)
                {
                    sid = _model._INWARD.id;
                    _Data.Inspection_ID = _model._INWARD.id.ToString();
                    _Data.JobNum = _model._INWARD.JobNo;
                    _Data.PartNum = _model._INWARD.Partno;
                    _Data.Inspection_Type = _model._INWARD.InspectionType;
                    _Data.Inspection_date = _model._submodel.inspectiondate;
                    _Data.DecisionType = "Rework";

                    //_Data.starttime = _model._submodel.StartTime;
                    _Data.Qty = _model._submodel.InspectedQty;
                    _Data.done_by = _model._submodel.InspectionBy;
                    if (_model._INWARD.reworkqty != null && _model._submodel.InspectedQty != null)
                    {
                        if (_model._INWARD.reworkqty == _model._submodel.InspectedQty)
                        {
                            _Data.CompleteRework = true;
                        }
                    }

                    DB.Final_Inspection_Process_Rework.Add(_Data);
                    DB.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "AddReworkDataData", "Home");
            }
            return RedirectToAction("reworkpagePage", new { id = sid });
        }
        public ActionResult AddDaviationkDataData(mAINPROGRESSModel _model)
        {

            MainInwardModel inwardData = new MainInwardModel();
            Final_Inspection_Stage_Data _stage = new Final_Inspection_Stage_Data();
            Final_Inspection_Data _data = new Final_Inspection_Data();
            int sid = 0;
            int? _stages = null;
            try
            {
                Final_Inspection_Process_Rework _Data = new Final_Inspection_Process_Rework();
                if (_model != null)
                {
                    sid = _model._INWARD.id;
                    _Data.Inspection_ID = _model._INWARD.id.ToString();
                    _Data.JobNum = _model._INWARD.JobNo;
                    _Data.PartNum = _model._INWARD.Partno;
                    _Data.Inspection_Type = _model._INWARD.InspectionType;
                    _Data.Inspection_date = _model._submodel.inspectiondate;
                    _Data.DecisionType = "Sorting";

                    //_Data.starttime = _model._submodel.StartTime;
                    _Data.Qty = _model._submodel.InspectedQty;
                    _Data.done_by = _model._submodel.InspectionBy;
                    if (_model._INWARD.reworkqty != null && _model._submodel.InspectedQty != null)
                    {
                        if (_model._INWARD.reworkqty == _model._submodel.InspectedQty)
                        {
                            _Data.CompleteRework = true;
                        }
                    }

                    DB.Final_Inspection_Process_Rework.Add(_Data);
                    DB.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "AddDaviationkDataData", "Home");
            }
            return RedirectToAction("SortingPage", new { id = sid });
        }
        public ActionResult ENDTIMEQTYRework(string id, string endtime, string qty)
        {
            try
            {
                if (id != null && endtime != null && qty != null)
                {
                    int ID = Convert.ToInt32(id);
                    DateTime? _edate = Convert.ToDateTime(endtime);
                    Final_Inspection_Process_Rework _Inspection_Data = DB.Final_Inspection_Process_Rework.Where(V => V.ID == ID).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
                        _Inspection_Data.endtime = _edate;
                        _Inspection_Data.Inspectionqty = Convert.ToInt32(qty);
                        DB.SaveChanges();
                    }
                    TempData["SuccessMessage"] = "Data saved successfully.";
                    //TempData["inspectiontype"] = instype;

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
                logService.AddLog(ex, "ENDTIMEQTYRework", "Home");
            }
            return RedirectToAction("Index");
        }
        public ActionResult Reworkstatuschange(mAINPROGRESSModel _model)
        {
            try
            {
                if (_model != null)
                {
                    int ID = _model._INWARD.id;
                    int stgeno = Convert.ToInt32(_model._submodel.Stage);
                    string _Stages = DB.Final_Inspection_Stage_Master.Where(l => l.Stage == stgeno).Select(l => l.stage_part_status).FirstOrDefault();

                    Final_Inspection_Data _Inspection_Data = DB.Final_Inspection_Data.Where(V => V.ID == ID).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
                        _Inspection_Data.Stage = _Stages;
                        _Inspection_Data.indeviation = false;
                        _Inspection_Data.completeandwaiting = false;
                    }
                    DB.SaveChanges();
                    Final_Inspection_Stage_Data _Stage_Data = new Final_Inspection_Stage_Data();
                    _Stage_Data.InspectionType = _model._INWARD.InspectionType;
                    _Stage_Data.Inspection_ID = _model._INWARD.id.ToString();
                    _Stage_Data.PartNum = _model._INWARD.Partno;
                    _Stage_Data.JobNum = _model._INWARD.JobNo;
                    _Stage_Data.Stage = _Inspection_Data.Stage;
                    _Stage_Data.Qty = _Inspection_Data.qty.ToString();
                    _Stage_Data.Active = true;
                    _Stage_Data.Deleted = false;
                    _Stage_Data.CurrentDateTime = DateTime.Now.ToString();
                    DB.Final_Inspection_Stage_Data.Add(_Stage_Data);


                    Final_Inspection_Data _I_Data = DB.Final_Inspection_Data.Where(V => V.ID == ID).FirstOrDefault();
                    if (_I_Data != null)
                    {
                        if (_Stages == "2 - Parts waiting for MRB")
                        {
                            _Inspection_Data.waitingformrb = true;
                        }
                        else if (_Stages == "5 - Parts in rework")
                        {
                            _Inspection_Data.inrework = true;
                        }
                        else if (_Stages == "4 - Parts waiting for rework")
                        {
                            _Inspection_Data.waitingforrework = true;
                        }
                        else if (_Stages == "3 - Parts waitng for sorting")
                        {
                            _Inspection_Data.waitingforsorting = true;
                        }
                        else if (_Stages == "7 - Parts in deviation")
                        {
                            _Inspection_Data.indeviation = true;
                        }
                        else if (_Stages == "10 - Parts Ready For packing")
                        {
                            _Inspection_Data.readyforpacking = true;
                        }
                        else if (_Stages == "11 - Parts moved from Quality")
                        {
                            _Inspection_Data.movedfromquality = true;
                        }
                        else if (_Stages == "12 - Parts in Hold")
                        {
                            _Inspection_Data.Hold = true;
                        }
                        else if (_Stages == "8 - Parts don't have unit price and rev issue")
                        {
                            _Inspection_Data.unitprice = true;
                        }
                    }
                    var recordsToUpdate = DB.Final_Inspection_Mrb_Data
                     .Where(first => first.Gid == ID)
                     .Join(
                         DB.Final_Inspection_Mrb_Rcode,
                         first => first.ID,
                         second => second.PID,
                         (first, second) => second // Select the `Final_Inspection_Mrb_Rcode` record to update
                     );

                    foreach (var record in recordsToUpdate)
                    {
                        record.Rework = 0; // Set the `Rework` field to 0
                    }
                    DB.SaveChanges();

                   DB.Final_Inspection_Process_Rework.Where(p=>p.Inspection_ID == _model._INWARD.id.ToString() && p.DecisionType == "Rework").ToList()
                    .ForEach(item => item.reworkstatuschange = true);
                    DB.SaveChanges();

                    TempData["SuccessMessage"] = "Stage Change successfully.";

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
            }
            return RedirectToAction("Index");
        }
        public ActionResult sortingstatuschange(mAINPROGRESSModel _model)
        {
            try
            {
                if (_model != null)
                {
                    int ID = _model._INWARD.id;
                    int? id = int.TryParse(_model._submodel.Stage, out int tempId) ? tempId : (int?)null;
                    int? stgeno = id.HasValue ? id.Value : (int?)null;
                    string _Stages = stgeno.HasValue? DB.Final_Inspection_Stage_Master.Where(l => l.Stage == stgeno.Value).Select(l => l.stage_part_status).FirstOrDefault(): _model._submodel.Stage;

                    Final_Inspection_Data _Inspection_Data = DB.Final_Inspection_Data.Where(V => V.ID == ID).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
                        _Inspection_Data.Stage = _Stages;
                        _Inspection_Data.waitingforsorting = false;
                        _Inspection_Data.completeandwaiting = false;
                    }
                    Final_Inspection_Stage_Data _Stage_Data = new Final_Inspection_Stage_Data();
                    _Stage_Data.InspectionType = _model._INWARD.InspectionType;
                    _Stage_Data.Inspection_ID = _model._INWARD.id.ToString();
                    _Stage_Data.PartNum = _model._INWARD.Partno;
                    _Stage_Data.JobNum = _model._INWARD.JobNo;
                    _Stage_Data.Stage = _Inspection_Data.Stage;
                    _Stage_Data.Qty = _Inspection_Data.qty.ToString();
                    _Stage_Data.Active = true;
                    _Stage_Data.Deleted = false;
                    _Stage_Data.CurrentDateTime = DateTime.Now.ToString();
                    DB.Final_Inspection_Stage_Data.Add(_Stage_Data);


                    Final_Inspection_Data _I_Data = DB.Final_Inspection_Data.Where(V => V.ID == ID).FirstOrDefault();
                    if (_I_Data != null)
                    {
                        if (_Stages == "2 - Parts waiting for MRB")
                        {
                            _Inspection_Data.waitingformrb = false; // km k  aena uper thi gayu che k nai mrb a b decide thi che 
                        }
                        else if (_Stages == "5 - Parts in rework")
                        {
                            _Inspection_Data.inrework = true;
                        }
                        else if (_Stages == "4 - Parts waiting for rework")
                        {
                            _Inspection_Data.waitingforrework = true;
                        }
                        else if (_Stages == "3 - Parts waitng for sorting")
                        {
                            _Inspection_Data.waitingforsorting = true;
                        }
                        else if (_Stages == "7 - Parts in deviation")
                        {
                            _Inspection_Data.indeviation = true;
                        }
                        else if (_Stages == "10 - Parts Ready For packing")
                        {
                            _Inspection_Data.readyforpacking = true;
                        }
                        else if (_Stages == "11 - Parts moved from Quality")
                        {
                            _Inspection_Data.movedfromquality = true;
                        }
                        else if (_Stages == "12 - Parts in Hold")
                        {
                            _Inspection_Data.Hold = true;
                        }
                        else if (_Stages == "8 - Parts don't have unit price and rev issue")
                        {
                            _Inspection_Data.unitprice = true;
                        }
                    }
                    var recordsToUpdate = DB.Final_Inspection_Mrb_Data
                     .Where(first => first.Gid == ID)
                     .Join(
                         DB.Final_Inspection_Mrb_Rcode,
                         first => first.ID,
                         second => second.PID,
                         (first, second) => second // Select the `Final_Inspection_Mrb_Rcode` record to update
                     );

                    foreach (var record in recordsToUpdate)
                    {
                        record.Rework = 0; // Set the `Rework` field to 0
                    }
                    DB.SaveChanges();

                    DB.Final_Inspection_Process_Rework.Where(p => p.Inspection_ID == _model._INWARD.id.ToString() && p.DecisionType == "Sorting").ToList()
                     .ForEach(item => item.reworkstatuschange = true);
                    DB.SaveChanges();

                    TempData["SuccessMessage"] = "Stage Change successfully.";

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}