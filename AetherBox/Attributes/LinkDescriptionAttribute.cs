namespace AetherBox.Attributes;

using System;

/// <summary>
/// The link to an image or website.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class LinkDescriptionAttribute : Attribute
{
    /// <summary>
    /// The description.
    /// </summary>
    public LinkDescription LinkDescription { get; set; }

    /// <summary>
    /// Constructer.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="description"></param>
    public LinkDescriptionAttribute(string url, string description = "")
    {
        LinkDescription = new() { Url = url, Description = description };
    }
}

/// <summary>
/// Link description itself.
/// </summary>
public struct LinkDescription
{
    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Url.
    /// </summary>
    public string Url { get; init; }
}
