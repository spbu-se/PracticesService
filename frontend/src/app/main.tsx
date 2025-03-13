import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import "bootstrap-icons/font/bootstrap-icons.css";
import ReactDOM from 'react-dom/client'
import '../index.css'
import {RouterProvider} from 'react-router-dom'
import {routes} from "./routes/routes";

ReactDOM.createRoot(document.getElementById('root')!).render(
    <RouterProvider router={routes}/>,
)