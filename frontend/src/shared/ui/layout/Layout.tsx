import Header from "@shared/ui/layout/Header";
import {FunctionComponent, ReactElement} from "react";

// Base layout realisation
export const Layout: FunctionComponent<{ children: ReactElement }> = props => {
    return (
        <div>
            <Header/>
            {props.children}
        </div>
    );
};