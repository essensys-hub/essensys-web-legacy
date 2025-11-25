using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using Essensys.Repository.DTO;
using Essensys.Common;
using NHibernate.Criterion;

namespace Essensys.Repository
{
    /// <summary>
    /// Repository de base
    /// </summary>
    public class BaseRepository<T> where T : IEsObject
    {
        private readonly ISession _session;
        
        public BaseRepository(ISession session)
        {
            _session = session;
        }
        
        /// <summary>
        /// Ajoute un élément
        /// </summary>
        /// <param name="entity">Element</param>
        /// <returns>Identifiant de l'élément créé</returns>
        public int Add(T entity)
        {
            _session.Save(entity);
            return entity.Id;
        }

        /// <summary>
        /// Met à un jour un élément
        /// </summary>
        /// <param name="entity">Element</param>
        /// <returns></returns>
        public virtual bool Update(T entity)
        {
            _session.Update(entity);
            _session.Flush();
            return true;
        }

        /// <summary>
        /// Supprime un élément
        /// </summary>
        /// <param name="entity">Element</param>
        /// <returns></returns>
        public bool Delete(T entity)
        {
            int id = entity.Id;
            _session.Delete(entity);
            _session.Flush();

            // Mise à jour des caches Query et Deuxième niveau
            _session.SessionFactory.Evict(entity.GetType(), id);
            _session.SessionFactory.EvictQueries();
            return true;
        }

        /// <summary>
        /// Rafraichit l'élément en session
        /// </summary>
        /// <param name="entity">Element</param>
        public void Refresh(T entity)
        {
            _session.Refresh(entity);
        }

        /// <summary>
        /// Retourne un élément à partir de son identifiant
        /// </summary>
        /// <param name="id">Identifiant</param>
        /// <returns>Element</returns>
        public T FindBy(int id)
        {
            return _session.Get<T>(id);
        }

        /// <summary>
        /// Retourne un élément correspondant à une expression
        /// </summary>
        /// <param name="expression">Expression Linq</param>
        /// <returns>Element</returns>
        public T FindBy(System.Linq.Expressions.Expression<System.Func<T, bool>> expression)
        {
            if (expression != null)
                return _session.Query<T>().Cacheable().FirstOrDefault(expression);
            else
                return _session.Query<T>().FirstOrDefault();
        }

        /// <summary>
        /// Retourne un élément correspondant à un critère
        /// </summary>
        /// <param name="criteria">Critère</param>
        /// <returns>Element</returns>
        public T FindByCriteria(ICriteria criteria)
        {
            try
            {
                return criteria.UniqueResult<T>();
            }
            catch (Exception ex)
            {
                LogManager.LogTrace("FindByCriteria : Echec de UniqueResult, " + ex.Message, ex);
                return criteria.List<T>()[0];
            }
        }

        /// <summary>
        /// Liste les éléments correspondant à une expression
        /// </summary>
        /// <param name="expression">Expression Linq</param>
        /// <param name="start">Début de la pagination</param>
        /// <param name="length">Taille de la pagination</param>
        /// <returns>Collection correspondante</returns>
        public IQueryable<T> List(System.Linq.Expressions.Expression<System.Func<T, bool>> expression, int start, int length)
        {
            if (expression == null)
                throw new Exception("L'expression ne peut pas être nulle");
            else
            {
                IQueryable<T> res = _session.Query<T>().Cacheable().Where(expression).AsQueryable();
                return res;
            }
        }

        /// <summary>
        /// Retourne une liste d'objet correspondant à un critère et paginée
        /// </summary>
        /// <param name="criteria">Critère</param>
        /// <param name="start">Début de la pagination</param>
        /// <param name="length">Longueur paginée</param>
        /// <returns>Liste d'objet</returns>
        public IQueryable<T> ListByCriteria(ICriteria criteria, int? start, int? length)
        {
            int nstart = 0;
            int nlength = 20;
 
            if (nlength > 0)
                return criteria.SetCacheable(true).SetFirstResult(nstart)
                    .SetMaxResults(nlength)
                    .List<T>().AsQueryable();
            else
                return criteria.SetCacheable(true).List<T>().AsQueryable();
                
        }

        /// <summary>
        /// Retourne le nombre d'éléments correspondant à une expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>Nombre d'éléments correspondant</returns>
        public int Count(System.Linq.Expressions.Expression<System.Func<T, bool>> expression, string PropertyName)
        {
            return _session.Query<T>().Where(expression).Count();
        }

        /// <summary>
        /// Retourne le nombre d'éléments correspondant à un criteria
        /// </summary>
        /// <param name="expression">Criteria</param>
        /// <returns>Nombre d'éléments correspondant</returns>
        /// <param name="PropertyName">Propriété par rapport à laquelle faire le count</param>
        public virtual int CountByCriteria(ICriteria criteria, string PropertyName)
        {
            criteria.ClearOrders();
            return criteria.SetProjection(Projections.Count(PropertyName)).UniqueResult<int>();
        }
    }
}
