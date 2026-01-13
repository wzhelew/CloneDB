package com.example.clonedb.dto;

public class LoginResponse {
    private final boolean authenticated;

    public LoginResponse(boolean authenticated) {
        this.authenticated = authenticated;
    }

    public boolean isAuthenticated() {
        return authenticated;
    }
}
