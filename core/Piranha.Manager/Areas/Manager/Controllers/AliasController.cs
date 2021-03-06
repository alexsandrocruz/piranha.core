/*
 * Copyright (c) 2018 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piranha.Manager;
using Piranha.Models;
using Piranha.Services;

namespace Piranha.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class AliasController : ManagerAreaControllerBase
    {
        private const string COOKIE_SELECTEDSITE = "PiranhaManager_SelectedSite";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="api">The current api</param>
        public AliasController(IApi api) : base(api) { }

        /// <summary>
        /// Gets the alias view.
        /// </summary>
        [Route("manager/aliases/{siteId:Guid?}")]
        [Authorize(Policy = Permission.Aliases)]
        public IActionResult List(Guid? siteId = null)
        {
            if (!siteId.HasValue)
            {
                var site = Request.Cookies[COOKIE_SELECTEDSITE];
                if (!string.IsNullOrEmpty(site))
                {
                    siteId = new Guid(site);
                }
            }
            else
            {
                Response.Cookies.Append(COOKIE_SELECTEDSITE, siteId.ToString());
            }
            return View("List", Models.AliasListModel.Get(_api, siteId));
        }

        /// <summary>
        /// Adds a new alias.
        /// </summary>
        [Route("manager/alias/add")]
        [HttpPost]
        [Authorize(Policy = Permission.AliasesEdit)]
        public IActionResult Add(Models.AliasEditModel model)
        {
            try
            {
                _api.Aliases.Save(new Alias
                {
                    SiteId = model.SiteId,
                    AliasUrl = model.AliasUrl,
                    RedirectUrl = model.RedirectUrl,
                    Type = model.IsPermanent ? RedirectType.Permanent : RedirectType.Temporary
                });
                SuccessMessage("The new alias has been added");
            }
            catch (ArgumentException)
            {
                ErrorMessage("Both AliasUrl and RedirectUrl are mandatory");
            }
            catch
            {
                ErrorMessage("There already exists an alias with the given url");
            }
            return RedirectToAction("List", new { siteId = model.SiteId });
        }

        /// <summary>
        /// Deletes an alias.
        /// </summary>
        [Route("manager/alias/delete/{id:Guid}")]
        [Authorize(Policy = Permission.AliasesDelete)]
        public IActionResult Delete(Guid id)
        {
            var alias = _api.Aliases.GetById(id);

            if (alias != null)
            {
                _api.Aliases.Delete(alias.Id);
                SuccessMessage("The alias has been deleted");
                return RedirectToAction("List", new { siteId = alias.SiteId });
            }
            ErrorMessage("The alias could not be deleted");
            return RedirectToAction("List", new { siteId = "" });
        }
    }
}
