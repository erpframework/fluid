﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Fluid.Filters;
using Fluid.Values;
using Microsoft.Extensions.FileProviders;

namespace Fluid
{
    public class TemplateContext
    {
        // Scopes
        public static Scope GlobalScope = new Scope();

        public Scope LocalScope { get; private set; }
        
        // Filters
        public FilterCollection Filters { get; } = new FilterCollection();

        public static FilterCollection GlobalFilters { get; } = new FilterCollection();

        /// <summary>
        /// Used to define custom object on this instance to be used in filters and statements
        /// but which are not available from the template.
        /// </summary>
        public Dictionary<string, object> AmbientValues = new Dictionary<string, object>();

        // Members

        /// <summary>
        /// Represent a global list of object members than can be accessed in any template.
        /// </summary>
        /// <remarks>
        /// This property should only be set by static constructores to prevent concurrency issues.
        /// </remarks>
        public static IMemberAccessStrategy GlobalMemberAccessStrategy = new MemberAccessStrategy();

        public static IFileProvider GlobalFileProvider { get; set; } = new NullFileProvider();

        /// <summary>
        /// Represent a local list of object members than can be accessed with this context.
        /// </summary>
        public IMemberAccessStrategy MemberAccessStrategy = new MemberAccessStrategy(GlobalMemberAccessStrategy);

        public IFileProvider FileProvider { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> instance used to render locale values like dates and numbers.
        /// </summary>
        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets or sets the way to return the current date and time for the template.
        /// </summary>
        public Func<DateTimeOffset> Now { get; set; } = () => DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets a model object that is used to resolve properties in a template. This object is used if local and 
        /// global scopes are unsuccessfull.
        /// </summary>
        public object Model { get; set; }

        static TemplateContext()
        {
            // Global properties
            GlobalScope.SetValue("empty", NilValue.Empty);
            GlobalScope.SetValue("blank", new StringValue(""));

            // Initialize Global Filters
            GlobalFilters
                .WithArrayFilters()
                .WithStringFilters()
                .WithNumberFilters()
                .WithMiscFilters();
        }

        public TemplateContext()
        {
            LocalScope = new Scope(GlobalScope);
        }

        /// <summary>
        /// Creates a new isolated scope. After than any value added to this content object will be released once
        /// <see cref="ReleaseScope" /> is called. The previous scope is linked such that its values are still available.
        /// </summary>
        public void EnterChildScope()
        {
            LocalScope = LocalScope.EnterChildScope();
        }

        /// <summary>
        /// Exits the current scope that has been created by <see cref="EnterChildScope" />
        /// </summary>
        public void ReleaseScope()
        {
            LocalScope = LocalScope.ReleaseScope();

            if (LocalScope == null)
            {
                throw new InvalidOperationException();
            }
        }

        public FluidValue GetValue(string name)
        {
            return LocalScope.GetValue(name);
        }

        public void SetValue(string name, FluidValue value)
        {
            LocalScope.SetValue(name, value);
        }

        public void SetValue(string name, int value)
        {
            SetValue(name, new NumberValue(value));
        }

        public void SetValue(string name, string value)
        {
            SetValue(name, new StringValue(value));
        }

        public void SetValue(string name, bool value)
        {
            SetValue(name, new BooleanValue(value));
        }

        public void SetValue(string name, object value)
        {
            SetValue(name, FluidValue.Create(value));
        }

        public void SetValue<T>(string name, Func<T> factory)
        {
            SetValue(name, new FactoryValue<T>(factory));
        }
    }
}
