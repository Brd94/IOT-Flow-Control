typedef struct {
    int id;
    int not_synced_delta;
    String business_name;
    String address;
    String pc;
    String city;
    //int pcount;
    int pcount_server;

}anag __attribute__ ((packed));;

typedef struct {
    int id;
    int last_state;
    int current_state;
    long last_change;
}pin __attribute__ ((packed));;
