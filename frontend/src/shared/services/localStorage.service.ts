export const setJWTToken = (token: string) => localStorage.setItem("JWTToken", token)

export const getJWTToken = () => localStorage.getItem("JWTToken")

export const getMyId = () => localStorage.getItem("me")