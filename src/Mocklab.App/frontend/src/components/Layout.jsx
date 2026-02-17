import { useState, useCallback, useEffect } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Ripple } from 'primereact/ripple';
import { classNames } from 'primereact/utils';

export default function Layout() {
  const navigate = useNavigate();
  const location = useLocation();
  const [sidebarActive, setSidebarActive] = useState(true);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 991);

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth <= 991);
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  // Close sidebar on mobile when route changes
  useEffect(() => {
    if (isMobile) {
      setSidebarActive(false);
    }
  }, [location.pathname, isMobile]);

  const menuItems = [
    {
      label: 'Main',
      items: [
        { label: 'Mock Responses', icon: 'pi pi-fw pi-database', to: '/' },
      ]
    },
    {
      label: 'Resources',
      items: [
        { label: 'API Docs', icon: 'pi pi-fw pi-book', url: '/openapi/v1.json', target: '_blank' },
        { label: 'GitHub', icon: 'pi pi-fw pi-github', url: 'https://github.com', target: '_blank' },
      ]
    }
  ];

  const onMenuToggle = useCallback(() => {
    setSidebarActive(prev => !prev);
  }, []);

  const onMenuItemClick = useCallback((item) => {
    if (item.to) {
      navigate(item.to);
    }
  }, [navigate]);

  const isActiveRoute = (item) => {
    return item.to && location.pathname === item.to;
  };

  const wrapperClass = classNames('layout-wrapper', {
    'layout-static': !isMobile,
    'layout-static-inactive': !isMobile && !sidebarActive,
    'layout-mobile-active': isMobile && sidebarActive,
  });

  return (
    <div className={wrapperClass}>
      {/* Topbar */}
      <div className="layout-topbar">
        <button
          className="layout-topbar-button layout-menu-button"
          onClick={onMenuToggle}
          aria-label="Toggle Menu"
        >
          <i className="pi pi-bars"></i>
        </button>

        <div className="layout-topbar-logo" onClick={() => navigate('/')}>
          <i className="pi pi-server"></i>
          <span>Mocklab</span>
        </div>

        <ul className="layout-topbar-menu">
          <li>
            <a
              href="https://github.com"
              target="_blank"
              rel="noopener noreferrer"
              className="layout-topbar-button"
              title="GitHub"
            >
              <i className="pi pi-github"></i>
            </a>
          </li>
        </ul>
      </div>

      {/* Sidebar */}
      <div className="layout-sidebar">
        <ul className="layout-menu">
          {menuItems.map((section, sIdx) => (
            <li key={sIdx} className="layout-root-menuitem">
              <div className="layout-menuitem-root-text">{section.label}</div>
              <ul>
                {section.items.map((item, iIdx) => (
                  <li key={iIdx}>
                    {item.url ? (
                      <a
                        href={item.url}
                        target={item.target || '_self'}
                        rel={item.target === '_blank' ? 'noopener noreferrer' : undefined}
                        className="p-ripple"
                      >
                        <i className={classNames('layout-menuitem-icon', item.icon)}></i>
                        <span className="layout-menuitem-text">{item.label}</span>
                        {item.target === '_blank' && (
                          <i className="pi pi-external-link layout-menuitem-external"></i>
                        )}
                        <Ripple />
                      </a>
                    ) : (
                      <a
                        onClick={() => onMenuItemClick(item)}
                        className={classNames('p-ripple', { 'active-route': isActiveRoute(item) })}
                      >
                        <i className={classNames('layout-menuitem-icon', item.icon)}></i>
                        <span className="layout-menuitem-text">{item.label}</span>
                        <Ripple />
                      </a>
                    )}
                  </li>
                ))}
              </ul>
            </li>
          ))}
        </ul>
      </div>

      {/* Overlay for mobile */}
      {isMobile && sidebarActive && (
        <div className="layout-mask" onClick={() => setSidebarActive(false)}></div>
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
