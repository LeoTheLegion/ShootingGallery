using System.Collections.Generic;

namespace ShootingGallery.Core;

/// <summary>
/// Optional contract for entity components that can be configured from scene XML properties.
/// The data-driven <see cref="GameObject"/> calls <see cref="Configure"/> on each declared
/// component (before attaching it) and passes the element's &lt;Property&gt; values, so a
/// component like <c>LabelComponent</c> can pick up Text/Color/Scale straight from XML.
/// </summary>
public interface IConfigurableComponent
{
    /// <param name="props">The raw property values declared on the entity element.</param>
    void Configure(Dictionary<string, string> props);
}
