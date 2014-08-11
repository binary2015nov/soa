using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.ESB.UDDI.Core.DataAccess
{
    public class ServiceDirectoryDBAccess
    {
        private static ServiceDirectoryDCDataContext serviceDirectoryDC = new ServiceDirectoryDCDataContext();
        public ServiceDirectoryDBAccess()
        {
            //serviceDirectoryDC = new ServiceDirectoryDCDataContext();
        }

        #region �����ṩ��
        public static List<������ͼ> GetAllUddiView()
        {
            var views = from uddiview in serviceDirectoryDC.������ͼ
                        where uddiview.ҵ��ϵͳ != null
                        select uddiview;
            return views.ToList();
        }
        public static List<ҵ��ʵ��> GetAllBuinessEntity()
        {
            var enities = from BusinessEntity in serviceDirectoryDC.ҵ��ʵ��
                          where BusinessEntity.ҵ����� != null
                          select BusinessEntity;
            return enities.ToList();
        }

        public static Guid AddNewBuinessEnitiy(ҵ��ʵ�� Entity)
        {
            Guid newID = Guid.NewGuid();
            try
            {
                Entity.ҵ����� = Guid.NewGuid();
                serviceDirectoryDC.ҵ��ʵ��.InsertOnSubmit(Entity);
                serviceDirectoryDC.SubmitChanges();
            }
            catch
            {
                newID = Guid.Empty;
            }
            return newID;
        }

        public static bool UpdateBuinessEntity(ҵ��ʵ�� Entity)
        {
            bool isSubmitOk = true;
            try
            {
                var Entities = serviceDirectoryDC.ҵ��ʵ��.Where(p => p.ҵ����� == Entity.ҵ�����);
                foreach (var i in Entities)
                {
                    i.���� = Entity.����;
                    i.ҵ������ = Entity.ҵ������;
                }
                serviceDirectoryDC.SubmitChanges();
            }
            catch
            {
                isSubmitOk = false;
            }
            return isSubmitOk;
        }

        public static bool RemoveBuinessEntity(ҵ��ʵ�� Entity)
        {
            bool isSubmitOk = true;
            try
            {
                //serviceDirectoryDC.ҵ��ʵ��.Attach(Entity);

                serviceDirectoryDC.ҵ��ʵ��.DeleteOnSubmit(GetBuinessEntityByBID(Entity.ҵ�����));
                serviceDirectoryDC.SubmitChanges();
            }
            catch 
            {
                isSubmitOk = false;
            }
            return isSubmitOk;
        }

        public static ҵ��ʵ�� GetBuinessEntityByBID(Guid BID)
        {
            var BizEntity = serviceDirectoryDC.ҵ��ʵ��.Single(p => p.ҵ����� == BID);
            if (BizEntity != null)
            {
               return BizEntity;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region �������

        public static List<����> GetAllBusinessService()
        {
            var entities = from service in serviceDirectoryDC.����
                         where service.������� !=null
                         orderby service.ҵ�����
                         select service;
            return entities.ToList();
        }
        public static ���� GetServiceBySID(Guid SID)
        {
            var entity = from service in serviceDirectoryDC.����
                         where service.������� == SID
                         select service;
            if (entity.Count() > 0)
                return entity.ToArray()[0];
            else
                return null;
        }

        public static ���� GetServiceByName(string Name)
        {
            var entity = from service in serviceDirectoryDC.����
                         where service.�������� == Name
                         select service;
            if (entity.Count() > 0)
                return entity.ToArray()[0];
            else
                return null;
        }
        
        /// <summary>
        /// ���ݷ����ṩ�߱��������з���
        /// </summary>
        /// <param name="SID"></param>
        /// <returns></returns>
        public static List<����> GetServiceByBID(Guid BID)
        {
            var entities = from service in serviceDirectoryDC.����
                           where service.ҵ����� == BID
                           select service;
            return entities.ToList();
        }



        public static bool UpdateBusinessService(���� entity)
        {
            bool isUpdateOk = true;
            try
            {
                var entities = serviceDirectoryDC.����.Where(p => p.������� == entity.�������);
                entities.ToArray()[0].�������� = entity.��������;
                entities.ToArray()[0].�������� = entity.��������;
                entities.ToArray()[0].���˱��� = entity.���˱���;
                entities.ToArray()[0].ҵ����� = entity.ҵ�����;
                entities.ToArray()[0].���� = entity.����;
                serviceDirectoryDC.SubmitChanges();
            }
            catch 
            {
                isUpdateOk = false;
            }
            return isUpdateOk;
        }

        public static Guid AddBusinessService(���� entity)
        {
            Guid SID = Guid.NewGuid();
            try
            {
                entity.������� = SID;
                serviceDirectoryDC.����.InsertOnSubmit(entity);
                serviceDirectoryDC.SubmitChanges();
            }
            catch 
            {
                SID = Guid.Empty;
            }
            return SID;
        }

        public static bool RemoveBusinessService(���� entity)
        {
            bool isdeleteOk = true;
            try
            {
                serviceDirectoryDC.����.DeleteOnSubmit(GetServiceBySID(entity.�������));
                serviceDirectoryDC.SubmitChanges();

            }
            catch 
            {
                isdeleteOk = false;
            }
            return isdeleteOk;
        }
        #endregion 

        #region �������Ա

        public static Guid AddNewPersonal(���� personal)
        {
            Guid newId = Guid.NewGuid();
            try
            {
                personal.���˱��� = newId;
                serviceDirectoryDC.����.InsertOnSubmit(personal);
                serviceDirectoryDC.SubmitChanges();
            }
            catch
            {
                newId = Guid.Empty;
            }
            return newId;
        }

        public static List<����> GetAllPersonal()
        {
            var pers = from per in serviceDirectoryDC.����
                          where per.���˱��� != Guid.Empty
                          select per;
            return pers.ToList();
        }

        public static bool RemovePersonal(���� personal)
        {
            bool issucceed = true;
            try
            {
                serviceDirectoryDC.����.DeleteOnSubmit(GetPersonalByPID(personal.���˱���));
                serviceDirectoryDC.SubmitChanges();
            }
            catch
            {
                issucceed = false;
            }
            return issucceed;
        }

        public static bool UpdatePersonal(���� personal)
        {
             bool issucceed = true;
             try
             {
                 var pers = serviceDirectoryDC.����.Where(p => p.���˱��� == personal.���˱���);
                 foreach (var per in pers)
                 {
                     per.�绰 = personal.�绰;
                     per.���� = personal.����;
                     per.�ʼ���ַ = personal.�ʼ���ַ;
                     per.Ȩ�� = personal.Ȩ��;
                     per.�ʺ� = personal.�ʺ�;
                     //per.���˱��� = personal.���˱���;
                 }
                 serviceDirectoryDC.SubmitChanges();
             }
             catch
             {
                 issucceed = false;
             }
             return issucceed;
        }

        public static ���� GetPersonalByPID(Guid PID)
        {
            var per = serviceDirectoryDC.����.Single(p => p.���˱��� == PID);
            return per;
        }

        public static ���� GetPersonalByAccount(string PAccount)
        {
            var per = serviceDirectoryDC.����.Single(p => p.�ʺ� == PAccount);
            return per;
        }

        #endregion 

        #region �����ַ
        public static List<�����ַ> GetAllBindingTemplate()
        {
            var entities = from bindingTemplate in serviceDirectoryDC.�����ַ
                         where bindingTemplate.�����ַ���� !=null
                         select bindingTemplate;
            return entities.ToList();
        }

        public static �����ַ GetTemplateByTID(Guid TID)
        {
            var entity = from bindingTemplate in serviceDirectoryDC.�����ַ
                                   where bindingTemplate.�����ַ���� ==TID
                                   select bindingTemplate;
            if (entity.Count() > 0)
                return entity.ToArray()[0];
            else
                return null;
        }

        /// <summary>
        /// ���ݷ�������õ�ַ
        /// </summary>
        /// <param name="SID"></param>
        /// <returns></returns>
        public static List<�����ַ> GetTemplatBySID(Guid SID)
        {
            var entities = from bindingTemplate in serviceDirectoryDC.�����ַ
                                   where bindingTemplate.������� ==SID
                                   select bindingTemplate;
            return entities.ToList();
        }



        public static Guid AddBindingTemplate(�����ַ entity)
        {
            Guid BID = Guid.NewGuid();
            try
            {
                entity.�����ַ���� = BID;
                serviceDirectoryDC.�����ַ.InsertOnSubmit(entity);
                serviceDirectoryDC.SubmitChanges();
            }
            catch 
            {
                BID = Guid.Empty;
            }
            return BID;
        }

        public static bool RemoveBindingTemplate(�����ַ entity)
        {
            bool isdeleteOk = true;
            try
            {
                serviceDirectoryDC.�����ַ.DeleteOnSubmit(GetTemplateByTID(entity.�����ַ����));
                serviceDirectoryDC.SubmitChanges();

            }catch
            {
                isdeleteOk = false;
            }
            return isdeleteOk;
        }

        public static bool UpdateBindingTemplate(�����ַ entity)
        {
            bool isUpdateOk = true;
            try
            {
                var entities = serviceDirectoryDC.�����ַ.Where(p => p.�����ַ���� == entity.�����ַ����);
                entities.ToArray()[0].������� = entity.�������;
                entities.ToArray()[0].���ʵ�ַ = entity.���ʵ�ַ;
                entities.ToArray()[0].���� = entity.����;
                entities.ToArray()[0].״̬ = entity.״̬;
                entities.ToArray()[0].������ = entity.������;
                entities.ToArray()[0].�������� = entity.��������;
                serviceDirectoryDC.SubmitChanges();
            }catch 
            {
                isUpdateOk = false;
            }
            return isUpdateOk;
        }

        #endregion

        #region ����Լ��
        public static List<����Լ��> GetAllTModel()
        {
            var entities = from tModel in serviceDirectoryDC.����Լ��
                         where tModel.Լ������ !=null
                         select tModel;
            return entities.ToList();
        }

        public static ����Լ�� GetTModelByMID(Guid MID)
        {
            var entity = from tModel in serviceDirectoryDC.����Լ��
                         where tModel.Լ������ == MID
                         select tModel;
            return entity.ToArray()[0];
        }

        /// <summary>
        /// ���ݷ����ַ�����÷���Լ��
        /// </summary>
        /// <param name="SID"></param>
        /// <returns></returns>
        public static List<����Լ��> GetModelByBID(Guid BID)
        {
            var entities = from tModel in serviceDirectoryDC.����Լ��
                           where tModel.�����ַ���� == BID
                           select tModel;
            return entities.ToList();
        }



        public static bool UpdateTModel(����Լ�� entity)
        {
            bool isUpdateOk = true;
            try
            {
                var entities = serviceDirectoryDC.����Լ��.Where(p => p.Լ������ == entity.Լ������);
                entities.ToArray()[0].�����ַ���� = entity.�����ַ����;
                entities.ToArray()[0].ʾ�� = entity.ʾ��;
                entities.ToArray()[0].���� = entity.����;
                serviceDirectoryDC.SubmitChanges();
            }
            catch 
            {
                isUpdateOk = false;
            }
            return isUpdateOk;
        }

        public static Guid AddTModel(����Լ�� entity)
        {
            Guid MID = Guid.NewGuid();
            try
            {
                entity.Լ������ = MID;
                serviceDirectoryDC.����Լ��.InsertOnSubmit(entity);
                serviceDirectoryDC.SubmitChanges();
            }
            catch 
            {
                MID = Guid.Empty;
            }
            return MID;
        }

        public static bool RemoveTModel(����Լ�� entity)
        {
            bool isdeleteOk = true;
            try
            {
                serviceDirectoryDC.����Լ��.DeleteOnSubmit(GetTModelByMID(entity.Լ������));
                serviceDirectoryDC.SubmitChanges();

            }
            catch 
            {
                isdeleteOk = false;
            }
            return isdeleteOk;
        }
    #endregion

        }
}
