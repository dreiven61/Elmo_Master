
// commands for SetConnParameter / SetMainParameter
#define SVR_TCP_CMD_SETSOCKOPT         0
#define SVR_TCP_CMD_TASKPRIO           1
#define SVR_TCP_CMD_CLOSESOCKETTYPE    2
#define SVR_TCP_CMD_KEEPALIVEPARA      3

// sub commands for SetConnParameter / SetMainParameter

// sub cmds socket options
#define SVR_TCP_SOCKOPT_NAGLE         0
#define SVR_TCP_SOCKOPT_KEEPALIVE      1
#define SVR_TCP_SOCKOPT_DELAYEDACK     2
#define SVR_TCP_SOCKOPT_REUSEADDR      3

// sub cmd task priority
#define SVR_TCP_SUB_CMD_TASKPRIO       0

// sub cmds keep alive parameter
#define SVR_TCP_KEEPALIVE_KEEPIDLE     0
#define SVR_TCP_KEEPALIVE_KEEPINTVL    1
#define SVR_TCP_KEEPALIVE_KEEPCNT      2

// sub cmd close socket type
#define SVR_TCP_CLOSESOCKETTYPE        0