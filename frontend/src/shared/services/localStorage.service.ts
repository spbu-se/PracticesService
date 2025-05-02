export const setJWTToken = (token: string) => localStorage.setItem("JWTToken", token)

export const getJWTToken = () => localStorage.getItem("JWTToken")

export const getMyId = () => localStorage.getItem("me")

export const setRefreshToken = (token: string) => localStorage.setItem("refreshToken", token)

export const getRefreshToken = () => localStorage.getItem("refreshToken")