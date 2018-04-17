using FutureState.Data;
using FutureState.Data.Providers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace FutureState.Services.Web.Api
{
    /// <summary>
    ///     Base web service class used to expose crud services to callers.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The entity/model that is being exposed via the web service.
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     The entity key.
    /// </typeparam>
    [EnableCors(Startup.CorsPolicyName)]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class FsControllerBase<TEntity, TKey> : Controller
        where TEntity : class, IEntityMutableKey<TKey>
        where TKey : IEquatable<TKey>
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        protected readonly ProviderLinq<TEntity, TKey> _service;

        protected FsControllerBase(ProviderLinq<TEntity, TKey> service)
        {
            //BusinessUnitService service
            _service = service;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!this.ModelState.IsValid)
            {
                //todo: convert to an action filter ?
                var errors = new List<Error>();
                foreach (var value in this.ModelState.Values)
                    foreach (var er in value.Errors)
                        errors.Add(new Error(er?.Exception?.Message ?? er?.ErrorMessage));

                if (_logger.IsTraceEnabled)
                    foreach (var er in errors)
                        _logger.Trace(er); // print the errors through the error output


                throw new RuleException("Invalid action. See the error collection for more details.", errors);
            }
        }

        /// <summary>
        ///     Gets all entities recorded in the system.
        /// </summary>
        /// <returns></returns>
        [Route("get")]
        [HttpGet]
        public virtual async Task<TEntity[]> Get()
        {
            return await Task.Run(() => _service.GetAll().ToArray());
        }

        /// <summary>
        ///     Gets an entity by its id.
        /// </summary>
        /// <param name="id">The id of the entity to query for.</param>
        /// <returns></returns>
        [Route("getById/{id}")]
        [HttpGet]
        public virtual async Task<TEntity> GetById([FromRoute] TKey id)
        {
            return await Task.Run(() =>
            {
                var item = _service.GetById(id);

                return item;
            });
        }

        /// <summary>
        ///     Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns></returns>
        [Route("add")]
        [HttpPost]
        public virtual async Task Add([FromBody] TEntity entity)
        {
            await Task.Run(() => _service.Add(entity));
        }

        /// <summary>
        ///     Updates a given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [Route("update")]
        [HttpPut]
        public virtual async Task Update([FromBody] TEntity entity)
        {
            await Task.Run(() => _service.Update(entity));
        }

        /// <summary>
        ///     Removes an item by its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("removeById/{id}")]
        [HttpDelete]
        public virtual async Task RemoveById([FromRoute] TKey id)
        {
            await Task.Run(() => _service.RemoveById(id));
        }

        /// <summary>
        ///     Removes a set of entities by their ids.
        /// </summary>
        /// <param name="ids">The set of ids to remove.</param>
        /// <returns></returns>
        [Route("removeByIds")]
        [HttpDelete]
        public virtual async Task RemoveByIds([FromBody] IEnumerable<TKey> ids)
        {
            var idsArray = ids.ToArraySafe();

            await Task.Run(() => _service.Remove(m => idsArray.Any(n => n.Equals(m.Id)))); // todo optimize
        }

        /// <summary>
        ///     Validates an entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [Route("validate")]
        [HttpPost]
        public virtual async Task<IEnumerable<Error>> Validate([FromBody] TEntity entity)
        {
            return await Task.Run(() => _service.Validate(entity));
        }
    }
}
