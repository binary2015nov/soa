﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using NewLife.Log;
using XCode;
using XCode.Configuration;
using System.Data;

namespace ESB.Core.Entity
{
    /// <summary></summary>
    [ModelCheckMode(ModelCheckModes.CheckTableWhenFirstUse)]
    public class ServiceMonitor : ServiceMonitor<ServiceMonitor> { }
    
    /// <summary></summary>
    public partial class ServiceMonitor<TEntity> : Entity<TEntity> where TEntity : ServiceMonitor<TEntity>, new()
    {
        #region 对象操作﻿
        static ServiceMonitor()
        {
            // 用于引发基类的静态构造函数，所有层次的泛型实体类都应该有一个
            TEntity entity = new TEntity();
        }

        ///// <summary>已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert</summary>
        ///// <returns></returns>
        //public override Int32 Insert()
        //{
        //    return base.Insert();
        //}

        ///// <summary>已重载。在事务保护范围内处理业务，位于Valid之后</summary>
        ///// <returns></returns>
        //protected override Int32 OnInsert()
        //{
        //    return base.OnInsert();
        //}

        ///// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
        ///// <param name="isNew"></param>
        //public override void Valid(Boolean isNew)
        //{
        //    // 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
        //    if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(_.Name, _.Name.DisplayName + "无效！");
        //    if (!isNew && ID < 1) throw new ArgumentOutOfRangeException(_.ID, _.ID.DisplayName + "必须大于0！");

        //    // 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
        //    base.Valid(isNew);

        //    // 在新插入数据或者修改了指定字段时进行唯一性验证，CheckExist内部抛出参数异常
        //    if (isNew || Dirtys[_.Name]) CheckExist(_.Name);
        //    if (isNew || Dirtys[_.Name] || Dirtys[_.DbType]) CheckExist(_.Name, _.DbType);
        //    if ((isNew || Dirtys[_.Name]) && Exist(_.Name)) throw new ArgumentException(_.Name, "值为" + Name + "的" + _.Name.DisplayName + "已存在！");
        //}


        ///// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void InitData()
        //{
        //    base.InitData();

        //    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
        //    // Meta.Count是快速取得表记录数
        //    if (Meta.Count > 0) return;

        //    // 需要注意的是，如果该方法调用了其它实体类的首次数据库操作，目标实体类的数据初始化将会在同一个线程完成
        //    if (XTrace.Debug) XTrace.WriteLine("开始初始化{0}ServiceMonitor数据……", typeof(TEntity).Name);

        //    var entity = new ServiceMonitor();
        //    entity.Name = "admin";
        //    entity.Password = DataHelper.Hash("admin");
        //    entity.DisplayName = "管理员";
        //    entity.RoleID = 1;
        //    entity.IsEnable = true;
        //    entity.Insert();

        //    if (XTrace.Debug) XTrace.WriteLine("完成初始化{0}ServiceMonitor数据！", typeof(TEntity).Name);
        //}
        #endregion

        #region 扩展属性﻿
        /// <summary>
        /// 小时
        /// </summary>
        public Int32 Hour { get; set; }
        /// <summary>
        /// 分钟
        /// </summary>
        public Int32 Minute { get; set; }
        /// <summary>
        /// 服务提供者
        /// </summary>
        public String BusinessName { get; set; }
        #endregion

        #region 扩展查询﻿
        /// <summary>根据OID查找</summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static TEntity FindByOID(String oid)
        {
            if (Meta.Count >= 1000)
                return Find(_.OID, oid);
            else // 实体缓存
                return Meta.Cache.Entities.Find(_.OID, oid);
            // 单对象缓存
            //return Meta.SingleCache[oid];
        }

        /// <summary>
        /// 根据服务名和方法名查找服务当天监控数据
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static EntityList<TEntity> FindAllByServiceAndMethodToday(String serviceName, String methodName)
        {
            var whereExp = new WhereExpression();
            whereExp &= _.MonitorStamp < DateTime.Now & _.MonitorStamp > DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")) & _.ServiceName == serviceName;

            var orderExp = new OrderExpression();
            orderExp &= _.MonitorStamp.Asc();

            if (methodName != "*")
            {
                whereExp &= _.MethodName == methodName;
            }

            return FindAll(whereExp, orderExp, null, 0, 0);
        }
        #endregion

        #region 高级查询
        // 以下为自定义高级查询的例子

        ///// <summary>
        ///// 查询满足条件的记录集，分页、排序
        ///// </summary>
        ///// <param name="key">关键字</param>
        ///// <param name="orderClause">排序，不带Order By</param>
        ///// <param name="startRowIndex">开始行，0表示第一行</param>
        ///// <param name="maximumRows">最大返回行数，0表示所有行</param>
        ///// <returns>实体集</returns>
        //[DataObjectMethod(DataObjectMethodType.Select, true)]
        //public static EntityList<TEntity> Search(String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
        //{
        //    return FindAll(SearchWhere(key), orderClause, null, startRowIndex, maximumRows);
        //}

        ///// <summary>
        ///// 查询满足条件的记录总数，分页和排序无效，带参数是因为ObjectDataSource要求它跟Search统一
        ///// </summary>
        ///// <param name="key">关键字</param>
        ///// <param name="orderClause">排序，不带Order By</param>
        ///// <param name="startRowIndex">开始行，0表示第一行</param>
        ///// <param name="maximumRows">最大返回行数，0表示所有行</param>
        ///// <returns>记录数</returns>
        //public static Int32 SearchCount(String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
        //{
        //    return FindCount(SearchWhere(key), null, null, 0, 0);
        //}

        /// <summary>构造搜索条件</summary>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        private static String SearchWhere(String key)
        {
            // WhereExpression重载&和|运算符，作为And和Or的替代
            var exp = new WhereExpression();

            // SearchWhereByKeys系列方法用于构建针对字符串字段的模糊搜索
            if (!String.IsNullOrEmpty(key)) SearchWhereByKeys(exp.Builder, key);

            // 以下仅为演示，2、3行是同一个意思的不同写法，Field（继承自FieldItem）重载了==、!=、>、<、>=、<=等运算符（第4行）
            //exp &= _.Name == "testName"
            //    & !String.IsNullOrEmpty(key) & _.Name == key
            //    .AndIf(!String.IsNullOrEmpty(key), _.Name == key)
            //    | _.ID > 0;

            return exp;
        }
        #endregion

        #region 扩展操作
        #endregion

        #region 业务
        /// <summary>
        /// 根据服务和方法名称统计监控数据
        /// </summary>
        /// <returns></returns>
        public static List<TEntity> GetMonitorServiceStatic()
        {
            var whereExp = new WhereExpression();
            whereExp &= _.MonitorStamp < DateTime.Now & _.MonitorStamp > DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));

            String where = whereExp + " Group by " + _.ServiceName + ", " + _.MethodName;
            String select = @" ServiceName, MethodName, SUM(CallSuccessNum) AS CallSuccessNum, SUM(CallHitCacheNum) AS CallHitCacheNum
                , SUM(CallFailureNum) AS CallFailureNum, SUM(CallQueueNum) AS CallQueueNum ";

            return FindAll(where, null, select, 0, 0);
        }

        /// <summary>
        /// 调用延时分析的统计数据
        /// </summary>
        /// <param name="businessID"></param>
        /// <returns></returns>
        public static EntityList<ServiceMonitor> GetInvokeAnalyse(String businessID)
        {
            String sql = String.Format("EXEC InvokeAnalyse '{0}','{1}', '{2}'"
                , DateTime.Now.ToString("yyyy-MM-dd 00:00:00")
                , DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                , businessID);

            DataSet dsMonitor = Meta.Query(sql);

            return ServiceMonitor.LoadData(dsMonitor);
        }

        /// <summary>
        /// 调用延时分析的统计数据
        /// </summary>
        /// <param name="businessID"></param>
        /// <returns></returns>
        public static EntityList<ServiceMonitor> GetInvokeTopService(String businessID)
        {
            String sql = String.Format("EXEC InvokeTopService '{0}','{1}', '{2}'"
                , DateTime.Now.ToString("yyyy-MM-dd 00:00:00")
                , DateTime.Now.ToString("yyyy-MM-dd 23:59:59")
                , businessID);

            DataSet dsTopServcie = Meta.Query(sql);

            return ServiceMonitor.LoadData(dsTopServcie);
        }
        #endregion
    }
}