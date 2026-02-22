import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { PrimeReactProvider } from 'primereact/api';
import Layout from './components/Layout';
import MockManagementPage from './pages/MockManagementPage';
import RequestLogsPage from './pages/RequestLogsPage';
import CollectionsPage from './pages/CollectionsPage';
import DataBucketsPage from './pages/DataBucketsPage';

// PrimeReact styles
import 'primereact/resources/themes/lara-light-indigo/theme.css';
import 'primereact/resources/primereact.min.css';
import 'primeicons/primeicons.css';
import 'primeflex/primeflex.css';

import './styles/layout/layout.scss';

function App() {
  return (
    <PrimeReactProvider>
      <BrowserRouter basename="/_admin">
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<MockManagementPage />} />
            <Route path="logs" element={<RequestLogsPage />} />
            <Route path="collections" element={<CollectionsPage />} />
            <Route path="data-buckets/:collectionId" element={<DataBucketsPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </PrimeReactProvider>
  );
}

export default App;
