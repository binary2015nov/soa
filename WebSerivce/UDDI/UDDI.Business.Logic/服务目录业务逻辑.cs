using System;
using System.Collections.Generic;
using System.Text;
using JN.ESB.Core.Service.Common;
using JN.ESB.UDDI.Core.DataAccess;

namespace JN.ESB.UDDI.Business.Logic
{
    public class ����Ŀ¼ҵ���߼�
    {

        /// <summary>
        /// //Zhen
        /// </summary>
        public ����Ŀ¼ҵ���߼�()
        {
            
        }

        public void ��ӷ����ṩ��(ҵ��ʵ�� �����ṩ��)
        {
            try
            {
                Guid bId = ServiceDirectoryDBAccess.AddNewBuinessEnitiy(�����ṩ��);
                //var service = �����ṩ��.����;
                //List<����> list = service;
                //foreach (���� service in list)
                //{
                //    service.ҵ����� = bId;
                //    ���� person = new ����();
                //    person.���˱��� = service.���˱���.Value;
                //    this.����ṩ�������(person,service);
                //}
                //throw new System.NotImplementedException();
            }
            catch { }
        }

        public void ��ӷ������Ա(���� �������Ա)
        {
            ServiceDirectoryDBAccess.AddNewPersonal(�������Ա);
        }
        public void ����ṩ�������(���� �������Ա,���� �������)
        {

            Guid pId = Guid.Empty;
            String name = �������Ա.����;
            if ((�������Ա.���˱��� == null) || (�������Ա.���˱��� == Guid.Empty))
            {
                //if (this.���ҷ������Ա_�ؼ���(name).Count == 0)
                //{
                //    pId = ServiceDirectoryDBAccess.AddNewPersonal(�������Ա);

                //}
                //else
                //{
                //    pId = this.���ҷ������Ա_�ؼ���(name)[0].���˱���;
                    
                //}
                pId = ServiceDirectoryDBAccess.AddNewPersonal(�������Ա);
            }
            else
            {
                pId = �������Ա.���˱���;
            }
            �������.���˱��� = pId;
            //List<�����ַ> templates = (���ַ)�������.�����ַ;
            //Guid ����Ա���� = �������.���˱���.Value;
            Guid sId = ServiceDirectoryDBAccess.AddBusinessService(�������);
            
            //foreach(�����ַ template in templates)
            //{
            //    template.������� = sId;
            //    this.��ӷ����(template);
            //}
            //throw new System.NotImplementedException();
        }

        ////
        public List<����Լ��> ��÷���Լ���߷����ַ(�����ַ �󶨷���)
        {
            //List<����Լ��> ����Լ�� = new List<����Լ��>();

            return ServiceDirectoryDBAccess.GetModelByBID(�󶨷���.�����ַ����);
            //return ����Լ��;
        }

        public ����Լ�� ��÷���Լ����Լ������(Guid Լ������)
        {
            return ServiceDirectoryDBAccess.GetTModelByMID(Լ������);
        }
        public void ��ӷ����(�����ַ �����ַ)
        {
            Guid tId = ServiceDirectoryDBAccess.AddBindingTemplate(�����ַ);
            //List<����Լ��> tModels = (����Լ��)�����ַ.����Լ��;
            //foreach (����Լ�� tModel in tModels)
            //{
            //    tModel.�����ַ���� = tId;
            //    ServiceDirectoryDBAccess.AddTModel(tModel);
            //}

            //throw new System.NotImplementedException();
        }

        public void ��ӷ���Լ��(����Լ�� ����Լ��)
        {
            ServiceDirectoryDBAccess.AddTModel(����Լ��);
            //throw new System.NotImplementedException();
        }

        
        public List<����> ������з������Ա()
        {
            List<����> ��ѯ��� = new List<����>();
            var personals = ServiceDirectoryDBAccess.GetAllPersonal();
            foreach (var personal in personals)
            {
                ��ѯ���.Add(personal);
            }
            return ��ѯ���;
        }


        public List<ҵ��ʵ��> ������з����ṩ��()
        {
            List<ҵ��ʵ��> ��ѯ��� = new List<ҵ��ʵ��>();
            var �����ṩ�߼� = ServiceDirectoryDBAccess.GetAllBuinessEntity();
            foreach (var �����ṩ�� in �����ṩ�߼�)
            {
                ��ѯ���.Add(�����ṩ��);
            }
            return ��ѯ���;
        }

        /// <summary>
        /// //Zhen
        /// </summary>
        public List<����> ���ҷ������Ա_�ؼ���(string �ؼ���)
        {
            
            List<����> ��ѯ��� = new List<����>();
            var personals = ServiceDirectoryDBAccess.GetAllPersonal();
            foreach (var personal in personals)
            {
                if (personal.����.Contains(�ؼ���))
                {
                    ��ѯ���.Add(personal);
                    continue;
                }
                else if (personal.�ʼ���ַ.Contains(�ؼ���))
                {
                    ��ѯ���.Add(personal);
                    continue;
                }
                else if (personal.�绰.Contains(�ؼ���))
                {
                    ��ѯ���.Add(personal);
                    continue;
                }
                else
                {
                    continue;
                }
            }
            return ��ѯ���;
        }

        public List<ҵ��ʵ��> ���ҷ����ṩ��_�ؼ���(String �ؼ���) 
        {
            List<ҵ��ʵ��> ҵ��ʵ�弯 = new List<ҵ��ʵ��>();
            var ��ѯ��� = ServiceDirectoryDBAccess.GetAllBuinessEntity();
            foreach (var ��� in ��ѯ���)
            {
                if (���.ҵ������.Contains(�ؼ���))
                {
                    ҵ��ʵ�弯.Add(���);
                    continue;
                }
            }
            return ҵ��ʵ�弯;
        }


        public void �޸ķ����ṩ��(ҵ��ʵ�� �����ṩ��)
        {
            ServiceDirectoryDBAccess.UpdateBuinessEntity(�����ṩ��);
            //List<����> services = (����)�����ṩ��.����;
            //foreach(���� service in services)
            //{
            //    this.�޸ľ������(service);
            //}
            //throw new System.NotImplementedException();
        }

        public void ɾ�������ṩ��(ҵ��ʵ�� �����ṩ��)
        {
            Guid bId = �����ṩ��.ҵ�����;
            List<����> services = ServiceDirectoryDBAccess.GetServiceByBID(bId);
            foreach (���� service in services)
            {
                this.ɾ���������(service);
            }
            ServiceDirectoryDBAccess.RemoveBuinessEntity(�����ṩ��);
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// //Zhen
        /// Tony 2011-03-22 ��������ṩ��.ҵ�����Ϊ���򷵻����з���
        /// </summary>
        public List<����> ��þ������_�����ṩ��(ҵ��ʵ�� �����ṩ��)
        {
            List<����> ��ѯ��� = new List<����>();
            var service = ServiceDirectoryDBAccess.GetAllBusinessService();

            if (�����ṩ��.ҵ����� == Guid.Empty)
            {
                foreach (var ser in service)
                {
                    ��ѯ���.Add(ser);
                }
            }
            else
            {
                foreach (var ser in service)
                {
                    if (ser.ҵ����� == �����ṩ��.ҵ�����)
                    {
                        ��ѯ���.Add(ser);
                        continue;
                    }
                }
            }

            return ��ѯ���;
        }

        /// <summary>
        /// //Zhen
        /// </summary>
        public List<����> ���Ҿ������_�ؼ���(string �ؼ���)
        {
            List<����> ���� = new List<����>();
            var ��ѯ��� = ServiceDirectoryDBAccess.GetAllBusinessService();
            foreach (var ��� in ��ѯ���)
            {
                if (���.��������.Contains(�ؼ���))
                {
                    ����.Add(���);
                    continue;
                }
            }
            return ����;
        }

        public void �޸ľ������(���� �������)
        {
            ServiceDirectoryDBAccess.UpdateBusinessService(�������);
            //List<�����ַ> templates = (�����ַ)�������.�����ַ;
            //foreach (�����ַ template in templates)
            //{
            //    this.�޸İ󶨷���(template);
            //}
            ////throw new System.NotImplementedException();
        }

        public void �޸ķ���Լ��(����Լ�� �������Լ��)
        {
            ServiceDirectoryDBAccess.UpdateTModel(�������Լ��);
        }

        public void ɾ���������(���� �������)
        {
            
            //List<�����ַ> templates = (�����ַ)�������.�����ַ;
            Guid sId = �������.�������;
            List<�����ַ> templates = ServiceDirectoryDBAccess.GetTemplatBySID(sId);
            foreach (�����ַ template in templates)
            {
                this.ɾ���󶨷���(template);
            }
            ServiceDirectoryDBAccess.RemoveBusinessService(�������);
            //throw new System.NotImplementedException();
        }

        public void ɾ������Լ��(����Լ�� �������Լ��) 
        {
            ServiceDirectoryDBAccess.RemoveTModel(�������Լ��);
        }

        /// <summary>
        /// //Zhen
        /// </summary>
        public List<�����ַ> ��ð���Ϣ_�������(���� �������Ԫ)
        {
            List<�����ַ> �����ַ��ѯ��� =  new List<�����ַ>(); 
            var ��ѯ��� = ServiceDirectoryDBAccess.GetAllBindingTemplate();
            foreach (var ��ѯ in ��ѯ���)
            {
                if(��ѯ.�������.Value == �������Ԫ.�������)
                {
                    �����ַ��ѯ���.Add(��ѯ);
                }
            }
            return �����ַ��ѯ���;
        }

        public �����ַ ��ð���Ϣ_��������(string ��������)
        {

            ���� ������� = ServiceDirectoryDBAccess.GetServiceByName(��������);
            if (������� != null)
            {
                List<�����ַ> �����ַ��ѯ��� = ServiceDirectoryDBAccess.GetTemplatBySID(�������.�������);

                if (�����ַ��ѯ���.Count > 0)
                    return �����ַ��ѯ���[0];
                else
                    return null;
            }
            else
                return null;
        }

        public �����ַ ��ð���Ϣ_�����ַ����(Guid �����ַ����)
        {
            return ServiceDirectoryDBAccess.GetTemplateByTID(�����ַ����);
        }

        public List<������ͼ> ������з�����ͼ()
        {
            return ServiceDirectoryDBAccess.GetAllUddiView();
        }

        public List<ҵ��ʵ��> ��÷����ṩ��_�������(���� �������)
        {
            List<ҵ��ʵ��> �����ṩ�߲�ѯ��� = new List<ҵ��ʵ��>();
            var ��ѯ��� = ServiceDirectoryDBAccess.GetAllBuinessEntity();
            foreach (var ��ѯ in ��ѯ���)
            {
                if (��ѯ.ҵ����� == �������.ҵ�����)
                {
                    �����ṩ�߲�ѯ���.Add(��ѯ);
                }
            }
            return �����ṩ�߲�ѯ���;
        }

        public ҵ��ʵ�� ��÷����ṩ��_�����ṩ�߱���(Guid �����ṩ�߱���)
        {
            //List<ҵ��ʵ��> �����ṩ�߲�ѯ��� = new List<ҵ��ʵ��>();
            var ��ѯ��� = ServiceDirectoryDBAccess.GetBuinessEntityByBID(�����ṩ�߱���);
            return ��ѯ���;
        }

        public ���� ��þ������_�������(Guid �������)
        {
            //���� �����ѯ��� = new ����();
            var ��ѯ��� = ServiceDirectoryDBAccess.GetServiceBySID(�������);
            return ��ѯ���;
        }

        public ���� ��þ������_����Ϣ(�����ַ �������Ϣ)
        {
            
            �����ַ ��ַ = ServiceDirectoryDBAccess.GetTemplateByTID(�������Ϣ.�����ַ����);

            ���� �����ѯ��� = ServiceDirectoryDBAccess.GetServiceBySID(��ַ.�������.Value);
            return �����ѯ���;
        }

        

        /// <summary>
        /// //Zhen
        /// </summary>
        public List<�����ַ> ���Ұ󶨵�ַ_�ؼ���(string �ؼ���)
        {
            List<�����ַ> ��ѯ��� = new List<�����ַ>();
            var bindingAddress = ServiceDirectoryDBAccess.GetAllBindingTemplate();
            foreach (var address in bindingAddress)
            {
                if (address.����.Contains(�ؼ���))
                {
                    ��ѯ���.Add(address);
                }
                else if (address.���ʵ�ַ.Contains(�ؼ���))
                {
                    ��ѯ���.Add(address);
                }
                else if (address.��������.Contains(�ؼ���))
                {
                    ��ѯ���.Add(address);
                }
                else
                {
                    continue;
                }
            }
            return ��ѯ���;
        }

        public void �޸İ󶨷���(�����ַ ����Ϣ)
        {
            //List<����Լ��> tModels = (����Լ��)����Ϣ.����Լ��;
            //foreach (����Լ�� tModel in tModels) 
            //{
            //    ServiceDirectoryDBAccess.UpdateTModel(tModel);
            //}
            ServiceDirectoryDBAccess.UpdateBindingTemplate(����Ϣ);
            
        }

        /// <summary>
        /// </summary>
        public void ɾ���󶨷���(�����ַ ����Ϣ)
        {
            Guid tId = ����Ϣ.�����ַ����;
            List<����Լ��> tModels = ServiceDirectoryDBAccess.GetModelByBID(tId);
            //List<����Լ��> tModels = ServiceDirectoryDBAccess.GetModelByBID(����Ϣ.�����ַ����);
            foreach(����Լ�� tModel in tModels)
            {
                ServiceDirectoryDBAccess.RemoveTModel(tModel);
            }
            ServiceDirectoryDBAccess.RemoveBindingTemplate(����Ϣ);
        }

        /// <summary>
        /// //Zhen
        /// </summary>
        public ���� ��ù���Ա_����󶨷���(�����ַ �󶨷���)
        {
           
            var ���� = ServiceDirectoryDBAccess.GetAllBusinessService().Find(p => p.������� == �󶨷���.�������);
            return ServiceDirectoryDBAccess.GetPersonalByPID(����.���˱���.Value);
        }

        public ���� ��ù���Ա_����Ա����(Guid ����Ա����)
        {
            return ServiceDirectoryDBAccess.GetPersonalByPID(����Ա����);
        }

        public ���� ��ù���Ա_����Ա�ʺ�(string ����Ա�ʺ�)
        {
            return ServiceDirectoryDBAccess.GetPersonalByAccount(����Ա�ʺ�);
        }

        public List<����> ���ϵͳ����Ա()
        {
            return ServiceDirectoryDBAccess.GetAllPersonal().FindAll(p => p.Ȩ�� == 0);
        }

        public void �޸ķ������Ա(���� ����Ա)
        {
            ServiceDirectoryDBAccess.UpdatePersonal(����Ա);
            //throw new System.NotImplementedException();
        }

        public void ɾ���������Ա(���� ����Ա)
        {
            ServiceDirectoryDBAccess.RemovePersonal(����Ա);
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// //Zhen
        /// </summary>
        public List<�����ַ> ���Ұ󶨷����ַ_����״̬(����״̬ ״̬)
        {
            List<�����ַ> ���ҽ�� = new List<�����ַ>();
            var services = ServiceDirectoryDBAccess.GetAllBindingTemplate();
            foreach (var ser in services)
            {
                if (ser.״̬.Value == (int)״̬)
                {
                    ���ҽ��.Add(ser);
                }
            }
            return ���ҽ��;

        }

        public List<�����ַ> ���Ұ󶨷����ַ_��������(�������� ����)
        {
            List<�����ַ> ���ҽ�� = new List<�����ַ>();
            var services = ServiceDirectoryDBAccess.GetAllBindingTemplate();
            foreach (var ser in services)
            {
                if (ser.������.Value == (int)����)
                {
                    ���ҽ��.Add(ser);
                }
            }
            return ���ҽ��;

        }

        public List<����> ��þ������_����Ա(���� ����Ա) 
        {
            List<����> services = new List<����>();
            foreach (ҵ��ʵ�� �ṩ�� in ������з����ṩ��()) {
                List<����> ser = ��þ������_�����ṩ��(�ṩ��);
                foreach (���� ������� in ser){
                    if (�������.���˱��� == ����Ա.���˱���)
                        services.Add(�������);
                }
            }
            return services;
        }

    }
}
