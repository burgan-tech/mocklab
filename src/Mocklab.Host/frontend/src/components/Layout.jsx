import { useState, useCallback, useEffect, useRef } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Ripple } from 'primereact/ripple';
import { classNames } from 'primereact/utils';
import { Badge } from 'primereact/badge';
import { requestLogService } from '../services/requestLogService';

export default function Layout() {
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileMenuActive, setMobileMenuActive] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 991);
  const [logCount, setLogCount] = useState(null);
  const menuRef = useRef(null);

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth <= 991);
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    const fetchCount = async () => {
      try {
        const result = await requestLogService.getLogs({ page: 1, pageSize: 1 });
        setLogCount(result.totalCount);
      } catch { /* ignore */ }
    };
    fetchCount();
    const interval = setInterval(fetchCount, 30000);
    return () => clearInterval(interval);
  }, []);

  // Close mobile menu on route change
  useEffect(() => {
    setMobileMenuActive(false);
  }, [location.pathname]);

  // Close mobile menu on outside click
  useEffect(() => {
    if (!mobileMenuActive) return;
    const handleClick = (e) => {
      if (menuRef.current && !menuRef.current.contains(e.target)) {
        setMobileMenuActive(false);
      }
    };
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, [mobileMenuActive]);

  const navItems = [
    { label: 'Mock Responses', icon: 'pi pi-database', to: '/' },
    { label: 'Request Logs', icon: 'pi pi-list', to: '/logs', badge: logCount > 0 ? String(logCount) : null },
    { label: 'Collections', icon: 'pi pi-folder', to: '/collections' },
  ];

  const resourceItems = [
    { label: 'API Docs', icon: 'pi pi-book', url: '/openapi/v1.json', target: '_blank' },
    { label: 'GitHub', icon: 'pi pi-github', url: 'https://github.com', target: '_blank' },
  ];

  const onMenuToggle = useCallback(() => {
    setMobileMenuActive(prev => !prev);
  }, []);

  const onNavItemClick = useCallback((item) => {
    if (item.to) {
      navigate(item.to);
    }
  }, [navigate]);

  const isActiveRoute = (item) => {
    if (item.to === '/') return location.pathname === '/';
    return item.to && location.pathname.startsWith(item.to);
  };

  return (
    <div className="layout-wrapper">
      {/* Topbar */}
      <div className="layout-topbar">
        <div className="layout-topbar-start">
          <div className="layout-topbar-logo" onClick={() => navigate('/')}>
            <i className="pi pi-server"></i>
            <div className="layout-topbar-logo-text">
              <span className="layout-topbar-app-name">Mocklab</span>
              <span className="layout-topbar-app-subtitle">API MOCK SERVER</span>
            </div>
          </div>
        </div>

        {/* Desktop navigation */}
        <nav className="layout-topbar-nav">
          {navItems.map((item, idx) => (
            <a
              key={idx}
              onClick={() => onNavItemClick(item)}
              className={classNames('layout-topbar-nav-item p-ripple', {
                'active-route': isActiveRoute(item),
              })}
            >
              <i className={item.icon}></i>
              <span>{item.label}</span>
              {item.badge && <Badge value={item.badge} severity="danger" />}
              <Ripple />
            </a>
          ))}
        </nav>

        <div className="layout-topbar-end">
          {resourceItems.map((item, idx) => (
            <a
              key={idx}
              href={item.url}
              target={item.target || '_self'}
              rel={item.target === '_blank' ? 'noopener noreferrer' : undefined}
              className="layout-topbar-button"
              title={item.label}
            >
              <i className={item.icon}></i>
            </a>
          ))}

          {/* Mobile hamburger */}
          <button
            className="layout-topbar-mobile-button"
            onClick={onMenuToggle}
            aria-label="Toggle Menu"
          >
            <i className={classNames(mobileMenuActive ? 'pi pi-times' : 'pi pi-bars')}></i>
          </button>
        </div>
      </div>

      {/* Mobile dropdown menu */}
      {isMobile && mobileMenuActive && (
        <>
          <div className="layout-mobile-mask" onClick={() => setMobileMenuActive(false)}></div>
          <div className="layout-mobile-menu" ref={menuRef}>
            <div className="layout-mobile-menu-section">
              <span className="layout-mobile-menu-label">Navigation</span>
              {navItems.map((item, idx) => (
                <a
                  key={idx}
                  onClick={() => onNavItemClick(item)}
                  className={classNames('layout-mobile-menu-item p-ripple', {
                    'active-route': isActiveRoute(item),
                  })}
                >
                  <i className={item.icon}></i>
                  <span>{item.label}</span>
                  {item.badge && <Badge value={item.badge} severity="danger" />}
                  <Ripple />
                </a>
              ))}
            </div>
            <div className="layout-mobile-menu-divider"></div>
            <div className="layout-mobile-menu-section">
              <span className="layout-mobile-menu-label">Resources</span>
              {resourceItems.map((item, idx) => (
                <a
                  key={idx}
                  href={item.url}
                  target={item.target || '_self'}
                  rel={item.target === '_blank' ? 'noopener noreferrer' : undefined}
                  className="layout-mobile-menu-item p-ripple"
                >
                  <i className={item.icon}></i>
                  <span>{item.label}</span>
                  {item.target === '_blank' && (
                    <i className="pi pi-external-link layout-mobile-menu-external"></i>
                  )}
                  <Ripple />
                </a>
              ))}
            </div>
          </div>
        </>
      )}

      {/* Main Content */}
      <div className="layout-main-container">
        <div className="layout-main">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
