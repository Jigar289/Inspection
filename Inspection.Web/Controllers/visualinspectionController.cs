using Inspection.Web.DataBase;
using Inspection.Web.Models;
using Inspection.Web.Service;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Media;

namespace Inspection.Web.Controllers
{
    [Authorize]
    public class visualinspectionController : Controller
    {
        // GET: visualinspection
        ITEIndiaEntities DB = new ITEIndiaEntities();
        InwardDataModel List = new InwardDataModel();
        List<InwardDataModel> _data = new List<InwardDataModel>();
        List<Submodel> _List = new List<Submodel>();
        Submodel _submodel = new Submodel();
        LogService logService = new LogService();
       [Authorize]
        public ActionResult Index(int? id, string value, string jobno , string stage)
        {
             int? totalacceptdqtyQty = null;
             int? totalfinalInspectedQty = null;
             int? totalvisualInspectedQty = null;
             int? totaltharedInspectedQty = null;
             bool? finalruning = false;
             bool? finalsts = false;
             bool? visualsts = false;
             bool? thareadsts = false;
             bool? fhumiditysts = false;
             int? totalhumidityInspectedQty = null;
             string currentstage = "";
            try
            {
                int finalqty = 0;
                var visualqty = "";
                var thareadqty = "";
                var fhumidityqty = "";
                string qstage = "";

                if (id != null && id != 0 && value != null)
                {
                    Session["uid"] = id;
                    Session["stage"] = value;
                    Session["jobno"] = jobno;
                    Session["stage"] = stage;
                }
                else
                {
                    if (id != 0 && id != null)
                    {
                        id = id = Convert.ToInt32(Session["uid"]);
                    }
                    value = value = Session["stage"].ToString();
                    jobno = jobno = Session["jobno"].ToString();
                    stage = stage = Session["stage"].ToString();
                    //qstage = qstage = Session["qualitystage"].ToString();
                }

                int? acceptQty = 0;
                int rejectQty = 0;
                var dataList = DB.Final_Inspection_Data.Where(p => p.JobNum == jobno && (p.QualityStage == stage || p.QualityStage.Contains(stage)) && p.Active == true && p.Delete == false).ToList();
                if (dataList != null && dataList.Count > 0)
                {
                    foreach (var data in dataList)
                    {
                        if (data.Inspection_Type == "Final")
                        {
                            finalqty = Convert.ToInt32(data.Sample_Qty);
                            finalsts = data.Statuschange != null ? data.Statuschange : false;
                            finalruning = data.FinalRuning != null ? data.FinalRuning : false;
                            if (data.CuurntCard != null)
                            {
                                currentstage = data.CuurntCard;
                            }
                        }
                        else if (data.Inspection_Type == "Visual")
                        {
                            visualqty = data.Sample_Qty;
                            visualsts = data.Statuschange != null ? data.Statuschange : false;
                            acceptQty += data.Accept_Qty ?? 0;
                            if (data.CuurntCard != null)
                            {
                                currentstage = data.CuurntCard;
                            }
                        }
                        else if (data.Inspection_Type == "Thread")
                        {
                            thareadqty = data.Sample_Qty;
                            thareadsts = data.Statuschange != null ? data.Statuschange : false;
                            acceptQty += data.Accept_Qty ?? 0;
                            if (data.CuurntCard != null)
                            {
                                currentstage = data.CuurntCard;
                            }
                        }
                        else if (data.Inspection_Type == "Humidity")
                        {
                            fhumidityqty = data.Sample_Qty;
                            fhumiditysts = data.Statuschange != null ? data.Statuschange : false;
                            if (data.CuurntCard != null)
                            {
                                currentstage = data.CuurntCard;
                            }
                        }
                    }
                }
                if (dataList != null)
                {
                    string inspectionQty = dataList.Select(p => p.Inspection_Qty).FirstOrDefault();
                    int _insqty = Convert.ToInt32(inspectionQty);
                    int _rejectQty = dataList.Where(p => p.Inspection_Type == "Visual" || p.Inspection_Type == "Thread").Sum(p => (p.Reject_Qty ?? 0));

                    acceptQty = _insqty - _rejectQty;
                }

                foreach (var data in dataList)
                {
                    var decisionDataList = DB.Final_Inspection_MRB_DecisionData
                               .Where(d => d.IID == data.ID)
                               .ToList();

                    rejectQty += decisionDataList
                                 .Where(d => !string.IsNullOrEmpty(d.RejectQty))
                                 .Sum(d => int.TryParse(d.RejectQty, out var qty) ? qty : 0);
                }
                List = (from model in DB.Final_Inspection_Data.Where(p => p.JobNum == jobno && (p.QualityStage == stage || p.QualityStage.Contains(stage)) && p.Active == true && p.Delete == false)
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
                            Statuschange = model.Statuschange,
                            SampleQuantity = model.Sample_Qty,
                            currentcard = model.CuurntCard,
                            acceptqty = acceptQty,
                            Note = model.Note,
                            currentstage = value,
                            finalinspection = finalqty,
                            visualinspection = visualqty,
                            threadinspection = thareadqty,
                            humidity = fhumidityqty,
                        }).FirstOrDefault();
                int insqty = Convert.ToInt32(List.Qty);
                acceptQty = insqty - rejectQty;

                _List = (from model in DB.Final_Inspection_Process.Where(p => p.JobNum == jobno && (p.Qualitystage == stage || p.Qualitystage.Contains(stage)) && p.Active == true && p.Deleted == false)
                         select new Submodel
                         {
                             id = model.ID,
                             inspectiondate = model.Inspection_date,
                             StartTime = model.starttime,
                             EndTime = model.endtime,
                             InspectedQty = model.Inspection_Qty,
                             InspectionBy = model.done_by,
                             InspectionTYPE = model.Inspection_Type,
                         }).ToList();

                //totalacceptdqtyQty = DB.Final_Inspection_Process.Where(p => p.JobNum == jobno && p.Inspection_Type == "Final");
                totalfinalInspectedQty = DB.Final_Inspection_Process.Where(p => p.JobNum == jobno && (p.Qualitystage == stage || p.Qualitystage.Contains(stage))  && p.Inspection_Type == "Final" && p.Active == true && p.Deleted == false).Sum(p => p.Inspection_Qty);
                totaltharedInspectedQty = DB.Final_Inspection_Process.Where(p => p.JobNum == jobno && (p.Qualitystage == stage || p.Qualitystage.Contains(stage)) && p.Inspection_Type == "Thread" && p.Active == true && p.Deleted == false).Sum(p => p.Inspection_Qty);
                totalvisualInspectedQty = DB.Final_Inspection_Process.Where(p => p.JobNum == jobno && (p.Qualitystage == stage || p.Qualitystage.Contains(stage)) && p.Inspection_Type == "Visual" && p.Active == true && p.Deleted == false).Sum(p => p.Inspection_Qty);
                totalhumidityInspectedQty = DB.Final_Inspection_Process.Where(p => p.JobNum == jobno && (p.Qualitystage == stage || p.Qualitystage.Contains(stage)) && p.Inspection_Type == "Humidity" && p.Active == true && p.Deleted == false).Sum(p => p.Inspection_Qty);
                
                if (jobno != null)
                {
                    _data = (from model in DB.Final_Inspection_Data.Where(l => l.JobNum == jobno && (l.QualityStage == stage || l.QualityStage.Contains(stage)) && l.Active == true && l.Delete == false)
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
                                 currentstage = value,
                                 currentcard = model.CuurntCard,
                                 QualityStage = model.QualityStage,
                             }).ToList();
                }
            }
            catch (Exception ex)
            {
                logService.AddLog(ex, "visualinspectionIndex", "visualinspectionController");
            }
             var maininwarddata = new mAINPROGRESSModel
             {
                _INWARD = List != null ? List : new InwardDataModel(),
                SUBMODEL = _List,
                TOTALfinalQTY = totalfinalInspectedQty != null ? totalfinalInspectedQty : 0,
                TOTALhumidityQTY = totalhumidityInspectedQty.ToString(),
                TOTALtharedQTY = totaltharedInspectedQty.ToString(),
                TOTALvisualQTY = totalvisualInspectedQty.ToString(),
                TOTALAcceptqtyforfinalQTY = totalacceptdqtyQty.ToString(),
                TOTALreworkQTY = 0,
                finalstatus   = finalsts,
                finalruning =  finalruning,
                visualstatus  = visualsts,
                tharedstatus  = thareadsts,
                humiditystatus = fhumiditysts,
                 currerntcard = currentstage,
                _INWARDList = _data,
                _submodel = new Submodel(),

            };
             return View(maininwarddata);
        }
        [Authorize]
        public ActionResult AddData(mAINPROGRESSModel _model)
        {
            try
            {
                Final_Inspection_Data _Data = new Final_Inspection_Data();
                int id = Convert.ToInt32(Session["uid"]);
                int ID = DB.Final_Inspection_Process.Where(p => p.MID == 1).Count() > 0 ? Convert.ToInt32(DB.Final_Inspection_Process.Where(p => p.MID == 1).Max(p => p.ID) + 1) : 1;

                if (_model._submodel != null)
                {
                    int _ID =0;

                    List<Final_Inspection_Data> inspectionData = DB.Final_Inspection_Data.Where(l => l.JobNum == _model._INWARD.JobNo && l.Active == true && l.Delete == false).ToList();
                    if (inspectionData.Any())
                    {
                        inspectionData.ForEach( item =>
                            {
                                item.CuurntCard = null;
                                DB.SaveChanges();
                        });
                    }

                    _Data = inspectionData.Where(l => l.Inspection_Type == _model._INWARD.InspectionType).FirstOrDefault();
                    if (inspectionData != null)
                    {
                        _ID = _Data.ID;
                        _Data.CuurntCard = _model._INWARD.InspectionType;
                    }

                    Final_Inspection_Process _Inspection_Data = new Final_Inspection_Process();
                    _Inspection_Data.PID = _ID;
                    _Inspection_Data.ID = ID;
                    _Inspection_Data.MID = 1;
                    _Inspection_Data.Inspection_ID = "2";
                    _Inspection_Data.Rework_Id = 0;
                    _Inspection_Data.Inspection_date = _model._submodel.inspectiondate;
                    _Inspection_Data.starttime = _model._submodel.StartTime;
                    _Inspection_Data.endtime = _model._submodel.EndTime;
                    _Inspection_Data.Inspection_Qty = Convert.ToInt32(_model._submodel.InspectedQty);
                    _Inspection_Data.done_by = _model._submodel.InspectionBy;
                    _Inspection_Data.JobNum = _model._INWARD.JobNo;
                    _Inspection_Data.PartNum = _model._INWARD.Partno;
                    _Inspection_Data.Statuschange = false;
                    _Inspection_Data.Stage = _model._INWARD.ProcessStage;
                    _Inspection_Data.Inspection_Type = _model._INWARD.InspectionType;
                    _Inspection_Data.sampleqty = _model._INWARD.SampleQuantity;
                    _Inspection_Data.Qualitystage = _model._INWARD.QualityStage;
                    _Inspection_Data.Active =true;
                    _Inspection_Data.Deleted = false;
                    DB.Final_Inspection_Process.Add(_Inspection_Data);
                    DB.SaveChanges();

                    //TempData["SuccessMessage"] = "Data saved successfully.";

                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]  
        public ActionResult ENDTIMEQTY(string id, string endtime, string qty , string instype ,int jobno)
        {
            try
            {
                if (id != null && endtime != null && qty != null)
                {
                    if (instype == "Final")
                    {
                        Final_Inspection_Data _Data = DB.Final_Inspection_Data.Where(k => k.ID == jobno).FirstOrDefault();
                        _Data.FinalRuning = false;
                    }
                    string qualitystage = DB.Final_Inspection_Data.Where(k => k.ID == jobno).Select(p=>p.QualityStage).FirstOrDefault();
                    DateTime? _edate = Convert.ToDateTime(endtime);
                    int ID = Convert.ToInt32(id);
                    Final_Inspection_Process _Inspection_Data = DB.Final_Inspection_Process.Where(V => V.ID == ID).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
                        _Inspection_Data.endtime = _edate;
                        _Inspection_Data.Inspection_Qty = Convert.ToInt32(qty);
                        DB.SaveChanges();
                    }
                    //TempData["SuccessMessage"] = "Data saved successfully.";
                    TempData["inspectiontype"] = instype;
                    Session["inspection_type"] = instype;

                    TempData["qualitystage"] = qualitystage;
                    Session["qualitystage"] = qualitystage;

                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult ENDTIMEQTYRuning(string id, string endtime, string qty, string instype, int jobno)
        {
            try
            {
                if (id != null && endtime != null && qty != null)
                {
                    if (jobno != null)
                    {
                        DateTime? _edate = Convert.ToDateTime(endtime);
                        Final_Inspection_Data _Data = DB.Final_Inspection_Data.Where(k=>k.ID == jobno).FirstOrDefault();
                        _Data.FinalRuning = true;
                        int ID = Convert.ToInt32(id);
                        Final_Inspection_Process _Inspection_Data = DB.Final_Inspection_Process.Where(V => V.ID == ID).FirstOrDefault();
                        if (_Inspection_Data != null)
                        {
                            _Inspection_Data.endtime = _edate;
                            _Inspection_Data.Inspection_Qty = Convert.ToInt32(qty);
                            DB.SaveChanges();
                        }
                        //TempData["SuccessMessage"] = "Data saved successfully.";
                    }
                    TempData["inspectiontype"] = instype;
                    Session["inspection_type"] = instype;

                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult statuschange(mAINPROGRESSModel _model)
        {                                                                                    
            try
            {
                if (_model != null)
                {
                    int? _stage = Convert.ToInt32(_model._submodel.Stage);
                    string _Stages = DB.Final_Inspection_Stage_Master.Where(l => l.ID == _stage).Select(l => l.stage_part_status).FirstOrDefault();
                    int ID = _model._INWARD.id;
                    Final_Inspection_Data _Inspection_Data = DB.Final_Inspection_Data.Where(V => V.JobNum == _model._INWARD.JobNo && V.QualityStage == _model._INWARD.QualityStage && V.Inspection_Type == _model._INWARD.InspectionType).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
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
                        
                        _Inspection_Data.Statuschange = true;
                        _Inspection_Data.Stage = _Stages;
                        if (_Stages == "2 - Parts waiting for MRB")
                        {
                            _Inspection_Data.Mrb_Create_date = DateTime.Now;
                        }
                        if (_model._submodel.Stage.Trim() == "16" || _model._submodel.Stage.Trim() == "15" || _model._submodel.Stage.Trim() == "18" || _model._submodel.Stage.Trim() == "11")
                        {
                            //int? accqty = Convert.ToInt16(_Inspection_Data.Sample_Qty);
                            int? accqty = Convert.ToInt16(_model._INWARD.Qty);
                            _Inspection_Data.Accept_Qty = accqty;
                        }
                        if (_Stages == "3 - Parts waiting for Sorting")
                        {
                            _Inspection_Data.Sorting_Qty = _model._submodel.DecisionQty;
                        }
                        else if (_Stages == "7 - Parts in Deviation")
                        {
                            _Inspection_Data.Deviation_Qty = _model._submodel.DecisionQty;
                            _Inspection_Data.indeviation = true;
                        }
                        else if (_Stages == "12 - Parts in Hold")
                        {
                            _Inspection_Data.Hold = true;
                        }
                        else if (_Stages == "10 - Parts Ready For Packing")
                        {
                            _Inspection_Data.Hold = true;
                        }

                    }
                    bool mrb = false;
                    bool rework = false;
                    bool sorting = false;
                    bool deviation = false;
                    bool packing = false;
                    bool hold = false;

                    List<Final_Inspection_Process> _I_Data = DB.Final_Inspection_Process.Where(V => V.JobNum == _model._INWARD.JobNo && V.Inspection_Type == _model._INWARD.InspectionType).ToList();
                    if (_I_Data != null)
                    {
                        if (_model._submodel.Stage.Trim() == "2 - Parts waiting for MRB" || _Stages == "2 - Parts waiting for MRB") 
                        {
                            mrb = true;
                        }
                        else if (_model._submodel.Stage.Trim() == "5 - Parts in rework" || _model._submodel.Stage.Trim() == "4 - Parts waiting for rework")
                        {
                            rework = true;
                        }
                        else if (_model._submodel.Stage.Trim() == "3 - Parts waitng for sorting")
                        {
                            sorting = true;
                        }
                        else if (_model._submodel.Stage.Trim() == "7 - Parts in deviation")
                        {
                            deviation = true;
                            //_Inspection_Data.Deviation_Qty == _I_Data
                        }
                        else if (_model._submodel.Stage.Trim() == "10 - Parts Ready For packing")
                        {
                            packing = true;
                        }
                        else if (_model._submodel.Stage.Trim() == "12 - Parts in Hold")
                        {
                            hold = true;
                        }
                        else if (_model._submodel.Stage.Trim() == "16 - Visual Inspection Completed")
                        {
                            int? __QTY = Convert.ToInt32(_Inspection_Data.Sample_Qty);
                            _Inspection_Data.Accept_Qty = (_Inspection_Data.Accept_Qty ?? 0) + (int.TryParse(_Inspection_Data.Sample_Qty, out int QTY) ? QTY : 0);
                        }
                        else if (_model._submodel.Stage.Trim() == "17 - Thread Inspection Completed")
                        {
                            int? _QTY = Convert.ToInt32(_model._submodel.SampleQuantity);
                            _Inspection_Data.Accept_Qty = (_Inspection_Data.Accept_Qty ?? 0) + (int.TryParse(_Inspection_Data.Sample_Qty, out int QTY) ? QTY : 0);
                        }
                        else if (_model._submodel.Stage.Trim() == "11 - Parts moved from quality")
                        {
                            int? _QTY = Convert.ToInt32(_model._submodel.SampleQuantity);
                            _Inspection_Data.Accept_Qty = (_Inspection_Data.Accept_Qty ?? 0) + (int.TryParse(_Inspection_Data.Sample_Qty, out int QTY) ? QTY : 0);
                        }
                        foreach (var item in _I_Data)
                        {
                            item.Stage = _model._submodel.Stage;
                            item.Statuschange = true;
                            //item.waitingMRB = mrb; 
                            //item.waitngsorting = sorting; 
                            //item.deviation = deviation; 
                            //item.reworkWAITING = rework; 
                            //item.packingwaiting = packing; 
                        }
                    }
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

        public ActionResult Addsampleqty(mAINPROGRESSModel _model)
        {
            try
            {
                if (_model != null)
                {
                    if (_model._INWARD != null)
                    {
                        if (_model._INWARD.typevalue == "Final")
                        {
                            int ID = _model._INWARD.id;
                            Final_Inspection_Data _Inspection_Data = DB.Final_Inspection_Data.Where(V => V.JobNum == _model._INWARD.JobNo && V.Inspection_Type == _model._INWARD.typevalue && V.QualityStage == _model._INWARD.QualityStage).FirstOrDefault();
                            if (_Inspection_Data != null)
                            {
                                _Inspection_Data.Sample_Qty = _model._submodel.SampleQuantity;
                                _Inspection_Data.MES = _model._submodel.MES;
                                _Inspection_Data.CuurntCard = _model._INWARD.typevalue;
                            }
                            DB.SaveChanges();
                        }
                        else
                        {
                            if (Convert.ToInt32(_model._INWARD.Qty) >= Convert.ToInt32(_model._submodel.SampleQuantity))
                            {
                                int ID = _model._INWARD.id;
                                Final_Inspection_Data _Inspection_Data = DB.Final_Inspection_Data.Where(V => V.JobNum == _model._INWARD.JobNo && V.Inspection_Type == _model._INWARD.typevalue && V.Active == true && V.Delete == false && V.QualityStage == _model._INWARD.QualityStage).FirstOrDefault();
                                if (_Inspection_Data != null)
                                {
                                    _Inspection_Data.Sample_Qty = _model._submodel.SampleQuantity;
                                    _Inspection_Data.MES = _model._submodel.MES;
                                    _Inspection_Data.CuurntCard = _model._INWARD.typevalue;
                                }
                                try
                                {
                                    DB.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] = "sample Qty id not valid.";
                            }
                        }
                    }
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string jobno)
        {
            MainInwardModel _Model = new MainInwardModel();

            _Model._INWARDList = (from model in DB.Final_Inspection_Data.Where(p => p.JobNum == jobno && p.Active == true && p.Delete == false && p.Active == true && p.Delete == false)
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
                    }
                    ).ToList();

            var maindata = new MainInwardModel
            {
                _submodel = new List<Submodel>(),
                _INWARD =  new InwardDataModel(),
                _INWARDList = _Model._INWARDList,
            };


            return PartialView("_Edit", maindata);
        }

        public ActionResult _Edit(string jobno, string inspectiontype)
        {
            MainInwardModel _Model = new MainInwardModel();
            InwardDataModel Model = new InwardDataModel();

            Final_Inspection_Data _data = DB.Final_Inspection_Data.Where(l=>l.JobNum == inspectiontype && l.Inspection_Type == jobno ).FirstOrDefault();//&& l.Active == true && l.Delete==false

            if (_data != null) {
                Model.InwardDate = _data.Inward_Date;
                Model.InwardTime = _data.Inward_Time;
                Model.JobNo = _data.JobNum;
                Model.Partno = _data.PartNum;
                Model.Qty = _data.Inspection_Qty;
                Model.InspectionType = _data.Inspection_Type;
                Model.ProcessStage = _data.Stage;
                Model.QualityStage = _data.QualityStage;
                Model.ActualRev = _data.ActRev;
                Model.ERev = _data.EpiRev;
            }
            _Model._INWARD = Model;
            return PartialView("_Editinward", _Model);
        }

        public ActionResult EditInwardData(MainInwardModel _model)
        {
            try
            {
                if (_model != null)
                {
                    Final_Inspection_Data _data = DB.Final_Inspection_Data.Where(k => k.JobNum == _model._INWARD.JobNo && k.Inspection_Type == _model._INWARD.InspectionType).FirstOrDefault();
                    if (_data != null)
                    {
                        _data.Inward_Date = _model._INWARD.InwardDate;
                        _data.Inward_Time = _model._INWARD.InwardTime;
                        _data.Inspection_Qty = _model._INWARD.Qty;
                        _data.ActRev = _model._INWARD.ActualRev;
                        _data.Stage = _model._INWARD.ProcessStage;
                        _data.QualityStage = _model._INWARD.QualityStage;
                        DB.SaveChanges();
                    }

                    TempData["SuccessMessage"] = "Data Updeted successfully.";

                    return RedirectToAction("Edit", new { jobno = _model._INWARD.JobNo });
                }
            }
            catch (Exception)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
            }
            return RedirectToAction("Edit");
        }

        public ActionResult _Delete(string jobno, string inspectiontype)
        {
            try
            {
                MainInwardModel _Model = new MainInwardModel();
                InwardDataModel Model = new InwardDataModel();

                Final_Inspection_Data _data = DB.Final_Inspection_Data.Where(l => l.JobNum == inspectiontype && l.Inspection_Type == jobno && l.Active == true && l.Delete == false).FirstOrDefault();

                if (_data != null)
                {
                    _data.Active = false;
                    _data.Delete = true;
                    DB.SaveChanges();
                }

                List<Final_Inspection_Process> _Inspection_Processes = DB.Final_Inspection_Process.Where(l => l.PID == _data.ID).ToList();
                if (_Inspection_Processes.Any())
                {
                    _Inspection_Processes.ForEach(item =>
                    {
                        item.Active = false;
                        item.Deleted = true;

                        DB.SaveChanges();
                    });
                }
                

                TempData["DeleteMessage"] = "Data Deleted successfully."; ;

                return RedirectToAction("Edit", new { jobno = jobno });
            }
            catch (Exception)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Deleted.";

            }
            return View();
        }

        public ActionResult ENDTIMEQTYEdit(string id, string endtime, string qty, string instype)
        {
            try
            {
                if (id != null && endtime != null && qty != null)
                {
                    int ID = Convert.ToInt32(id);
                    DateTime? _edate = Convert.ToDateTime(endtime);
                    Final_Inspection_Process _Inspection_Data = DB.Final_Inspection_Process.Where(V => V.ID == ID).FirstOrDefault();
                    if (_Inspection_Data != null)
                    {
                        if (endtime != "undefined")
                        {
                            _Inspection_Data.endtime = _edate;
                        }
                        _Inspection_Data.Inspection_Qty = Convert.ToInt32(qty);
                        DB.SaveChanges();
                    }
                    TempData["SuccessMessage"] = "Data saved successfully.";
                    TempData["inspectiontype"] = instype;

                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                TempData["WarningMessage"] = "Warning: Something went wrong Data Not Saved.";
            }
            return RedirectToAction("Index");
        }
    }
}