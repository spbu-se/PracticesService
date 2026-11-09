import ReactDOM from 'react-dom/client'; // Correct import for React 18
import '../index.css';
import { RouterProvider } from 'react-router-dom';
import { preloadPages, routes } from './routes/routes';
import { ThemeProvider } from '@mui/material/styles';
import { lightTheme } from '@/shared/ui/theme';
import { CssBaseline } from '@mui/material';
import { NotificationProvider } from '@/shared/ui/notifications/NotificationProvider';


const root = ReactDOM.createRoot(document.getElementById('root')!);

root.render(
    <ThemeProvider theme={lightTheme}>
        <CssBaseline />
        <NotificationProvider>
            <RouterProvider router={routes} />
        </NotificationProvider>
    </ThemeProvider>
);

// After the first page is shown, load the other pages in the background.
const idle = window.requestIdleCallback ?? ((callback: () => void) => window.setTimeout(callback, 1500));
idle(preloadPages);
