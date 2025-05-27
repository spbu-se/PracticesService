import ReactDOM from 'react-dom/client'; // Correct import for React 18
import '../index.css';
import { RouterProvider } from 'react-router-dom';
import { routes } from './routes/routes';
import { ThemeProvider } from '@mui/material/styles';
import { lightTheme } from '@/shared/ui/theme';
import { CssBaseline } from '@mui/material';


const root = ReactDOM.createRoot(document.getElementById('root')!);

root.render(
    <ThemeProvider theme={lightTheme}>
        <CssBaseline />
        <RouterProvider router={routes} />
    </ThemeProvider>
);
