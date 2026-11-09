import { Grid, Skeleton } from "@mui/material";

/// Placeholder cards shown while the list of practices is loading, so that the page does not flash "no practices".
export function PracticeListSkeleton() {
    return (
        <Grid container spacing={3}>
            {[0, 1, 2].map((i) => (
                <Grid key={i} size={{ xs: 12, sm: 6, md: 4 }}>
                    <Skeleton variant="rounded" height={170} />
                </Grid>
            ))}
        </Grid>
    );
}
