﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Descriptions;
using FubuCore.Logging;
using FubuMVC.Core.Runtime;
using FubuCore;

namespace FubuMVC.Core.Security
{
    public interface IAuthorizationPolicyExecutor
    {
        AuthorizationRight IsAuthorized(IFubuRequest request, IEnumerable<IAuthorizationPolicy> policies);
    }

    public class AuthorizationPolicyExecutor : IAuthorizationPolicyExecutor
    {
        private readonly ILogger _logger;

        public AuthorizationPolicyExecutor(ILogger logger)
        {
            _logger = logger;
        }

        public virtual AuthorizationRight IsAuthorized(IFubuRequest request, IEnumerable<IAuthorizationPolicy> policies)
        {
            return IsAuthorized(request, policies, null);
        }

        protected AuthorizationRight IsAuthorized(IFubuRequest request, IEnumerable<IAuthorizationPolicy> policies,
                                                  Action<IAuthorizationPolicy, AuthorizationRight> rightsDiscoveryAction)
        {
            // Check every authorization policy for this endpoint
            var rights = policies.Select(policy =>
            {
                var policyRights = policy.RightsFor(request);


                if (rightsDiscoveryAction != null)
                {
                    rightsDiscoveryAction(policy, policyRights);
                }

                _logger.DebugMessage(() => new AuthorizationPolicyResult(policy, policyRights));

                return policyRights;
            });

            // Combine the results
            var result = AuthorizationRight.Combine(rights);
            _logger.DebugMessage(() => new AuthorizationResult(result));

            return result;
        }
    }

    public class AuthorizationResult : LogRecord, DescribesItself
    {
        private readonly AuthorizationRight _rights;

        public AuthorizationResult(AuthorizationRight rights)
        {
            _rights = rights;
        }

        public void Describe(Description description)
        {
            description.Title = "Authorization result was '{0}'".ToFormat(_rights.Name);
        }

        public AuthorizationRight Rights
        {
            get { return _rights; }
        }

        public bool Equals(AuthorizationResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._rights, _rights);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (AuthorizationResult)) return false;
            return Equals((AuthorizationResult) obj);
        }

        public override int GetHashCode()
        {
            return (_rights != null ? _rights.GetHashCode() : 0);
        }
    }

    public class AuthorizationPolicyResult : LogRecord, DescribesItself
    {
        private readonly AuthorizationRight _rights;
        private readonly Description _policy;

        public AuthorizationPolicyResult(IAuthorizationPolicy policy, AuthorizationRight rights)
        {
            _rights = rights;
            _policy = Description.For(policy);
        }

        public AuthorizationRight Rights
        {
            get { return _rights; }
        }

        public Description Policy
        {
            get { return _policy; }
        }

        public void Describe(Description description)
        {
            description.Title = "Policy '{0}' returned authorization right '{1}'".ToFormat(_policy.Title, _rights.Name);
            description.Children["Policy"] = _policy;
            description.Properties["Rights"] = _rights.Name;
        }
    }
}