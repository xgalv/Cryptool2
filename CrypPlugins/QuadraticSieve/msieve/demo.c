/*--------------------------------------------------------------------
This source distribution is placed in the public domain by its author,
Jason Papadopoulos. You may use it for any purpose, free of charge,
without having to notify anyone. I disclaim any responsibility for any
errors.

Optionally, please be nice and tell me if you find this source to be
useful. Again optionally, if you add to the functionality present here
please consider making those additions public too, so that others may 
benefit from your work.	

$Id: demo.c 23 2009-07-20 02:59:07Z jasonp_sf $
--------------------------------------------------------------------*/

#include <msieve.h>
#include <signal.h>

msieve_obj *g_curr_factorization = NULL;

/*--------------------------------------------------------------------*/
void stop_msieve(msieve_obj *obj) {
	if (obj && (obj->flags & MSIEVE_FLAG_SIEVING_IN_PROGRESS))
		obj->flags |= MSIEVE_FLAG_STOP_SIEVING;
}

/*--------------------------------------------------------------------*/
void get_random_seeds(uint32 *seed1, uint32 *seed2) {

	uint32 tmp_seed1, tmp_seed2;

	/* In a multithreaded program, every msieve object
	   should have two unique, non-correlated seeds
	   chosen for it */

#ifndef WIN32

	FILE *rand_device = fopen("/dev/urandom", "r");

	if (rand_device != NULL) {

		/* Yay! Cryptographic-quality nondeterministic randomness! */

		fread(&tmp_seed1, sizeof(uint32), (size_t)1, rand_device);
		fread(&tmp_seed2, sizeof(uint32), (size_t)1, rand_device);
		fclose(rand_device);
	}
	else

#endif
	{
		/* <Shrug> For everyone else, sample the current time,
		   the high-res timer (hopefully not correlated to the
		   current time), and the process ID. Multithreaded
		   applications should fold in the thread ID too */

		uint64 high_res_time = read_clock();
		tmp_seed1 = ((uint32)(high_res_time >> 32) ^
			     (uint32)time(NULL)) * 
			    (uint32)getpid();
		tmp_seed2 = (uint32)high_res_time;
	}

	/* The final seeds are the result of a multiplicative
	   hash of the initial seeds */

	(*seed1) = tmp_seed1 * ((uint32)40499 * 65543);
	(*seed2) = tmp_seed2 * ((uint32)40499 * 65543);
}

/*--------------------------------------------------------------------*/
void print_usage(char *progname) {

	printf("\nMsieve v. %d.%02d\n", MSIEVE_MAJOR_VERSION, 
					MSIEVE_MINOR_VERSION);

	printf("\nusage: %s [options] [one_number]\n", progname);
	printf("\nnumbers starting with '0' are treated as octal,\n"
		"numbers starting with '0x' are treated as hexadecimal\n");
	printf("\noptions:\n"
	         "   -s <name> save intermediate results to <name>\n"
		 "             instead of the default %s\n"
	         "   -l <name> append log information to <name>\n"
		 "             instead of the default %s\n"
	         "   -i <name> read one or more integers to factor from\n"
		 "             <name> (default worktodo.ini) instead of\n"
		 "             from the command line\n"
		 "   -m        manual mode: enter numbers via standard input\n"
		 "   -mb <num> # of megabytes of memory for postprocessing \n"
		 "             (set automatically if unspecified or zero)\n"
	         "   -q        quiet: do not generate any log information,\n"
		 "             only print any factors found\n"
	         "   -d <min>  deadline: if still sieving after <min>\n"
		 "             minutes, shut down gracefully (default off)\n"
		 "   -r <num>  stop after finding <num> relations\n"
		 "   -p        run at idle priority\n"
	         "   -v        verbose: write log information to screen\n"
		 "             as well as to logfile\n"
	         "   -t <num>  use at most <num> threads\n\n"
		 " elliptic curve options:\n"
		 "   -e        perform 'deep' ECM, seek factors > 15 digits\n\n"
		 " quadratic sieve options:\n"
		 "   -c        client: only perform sieving\n\n"
		 " number field sieve options:\n"
		 "   -n        use the number field sieve (85+ digits only;\n"
		 "             performs all NFS tasks in order)\n"
	         "   -nf <name> read from / write to NFS factor base file\n"
		 "             <name> instead of the default %s\n"
		 "   -np [X,Y] perform only NFS polynomial selection; if\n"
		 "             specified, only cover leading coefficients\n"
		 "             in the range from X to Y inclusive\n"
		 "   -ns [X,Y] perform only NFS sieving; if specified,\n"
		 "             handle sieve lines X to Y inclusive\n"
		 "   -nc       perform only NFS combining (all phases)\n"
		 "   -nc1 [X,Y] perform only NFS filtering. Filtering will\n"
		 "             track ideals >= X (determined automatically\n"
		 "             if 0 or unspecified) and will only use the \n"
		 "             first Y relations (or all relations, if 0 \n"
		 "             or unspecified)\n"
		 "   -nc2      perform only NFS linear algebra\n"
		 "   -ncr      perform only NFS linear algebra, restarting\n"
		 "             from a previous checkpoint\n"
		 "   -nc3 [X,Y] perform only NFS square root (compute \n"
		 "             dependency numbers X through Y, 1<=X,Y<=64)\n",
		 MSIEVE_DEFAULT_SAVEFILE, MSIEVE_DEFAULT_LOGFILE,
		 MSIEVE_DEFAULT_NFS_FBFILE);
}

/*--------------------------------------------------------------------*/
msieve_factor* factor_integer(char *buf, uint32 flags,
		    char *savefile_name,
		    char *logfile_name,
		    char *nfs_fbfile_name,
		    uint32 *seed1, uint32 *seed2,
		    uint32 max_relations,
		    uint64 nfs_lower,
		    uint64 nfs_upper,
		    enum cpu_type cpu,
		    uint32 cache_size1,
		    uint32 cache_size2,
		    uint32 num_threads,
		    uint32 mem_mb) {
	
	char *int_start, *last;
	msieve_obj *obj;
	msieve_obj *g_curr_factorization;
	msieve_factor *factor;

	/* point to the start of the integer or expression;
	   if the start point indicates no integer is present,
	   don't try to factor it :) */

	last = strchr(buf, '\n');
	if (last)
		*last = 0;
	int_start = buf;
	while (*int_start && !isdigit(*int_start) &&
			*int_start != '(' ) {
		int_start++;
	}
	if (*int_start == 0)
		return 0;

	g_curr_factorization = msieve_obj_new(int_start, flags,
					savefile_name, logfile_name,
					nfs_fbfile_name,
					*seed1, *seed2, max_relations,
					nfs_lower, nfs_upper, cpu,
					cache_size1, cache_size2,
					num_threads, mem_mb);
	if (g_curr_factorization == NULL) {
		printf("factoring initialization failed\n");
		return 0;
	}

	msieve_run(g_curr_factorization);

	if (!(g_curr_factorization->flags & MSIEVE_FLAG_FACTORIZATION_DONE)) {
		printf("\ncurrent factorization was interrupted\n");
		//exit(0);
		if (g_curr_factorization)
			msieve_obj_free(g_curr_factorization);
		return 0;
	}

	/* If no logging is specified, at least print out the
	   factors that were found */

	if (!(g_curr_factorization->flags & (MSIEVE_FLAG_USE_LOGFILE |
					MSIEVE_FLAG_LOG_TO_STDOUT))) {
		factor = g_curr_factorization->factors;

		printf("\n");
		printf("%s\n", buf);
		while (factor != NULL) {
			char *factor_type;

			if (factor->factor_type == MSIEVE_PRIME)
				factor_type = "p";
			else if (factor->factor_type == MSIEVE_COMPOSITE)
				factor_type = "c";
			else
				factor_type = "prp";

			printf("%s%d: %s\n", factor_type, 
					(int32)strlen(factor->number), 
					factor->number);
			factor = factor->next;
		}
		printf("\n");
	}

	/* save the current value of the random seeds, so that
	   the next factorization will pick up the pseudorandom
	   sequence where this factorization left off */

	*seed1 = g_curr_factorization->seed1;
	*seed2 = g_curr_factorization->seed2;

	/*factor = g_curr_factorization->factors;
	g_curr_factorization->factors = NULL;*/

	/* free the current factorization struct. The following
	   avoids a race condition in the signal handler */

	obj = g_curr_factorization;
	g_curr_factorization = NULL;

	if (obj)
		msieve_obj_free(obj);

	return 0;
}

#ifdef WIN32
DWORD WINAPI countdown_thread(LPVOID pminutes) {
	DWORD minutes = *(DWORD *)pminutes;

	if (minutes > 0x7fffffff / 60000)
		minutes = 0;            /* infinite */

	Sleep(minutes * 60000);
	raise(SIGINT);
	return 0;
}

#else
void *countdown_thread(void *pminutes) {
	uint32 minutes = *(uint32 *)pminutes;

	if (minutes > 0xffffffff / 60)
		minutes = 0xffffffff / 60;   /* infinite */

	sleep(minutes * 60);
	raise(SIGINT);
	return NULL;
}
#endif

/*--------------------------------------------------------------------*/

char* getNextFactor(msieve_factor** factor) {
	char* n = (*factor)->number;	
	*factor = (*factor)->next;
	return n;
}


msieve_factor* factor(char *number, char *savefile_name) {	
	char buf[400];
	uint32 seed1, seed2;
	char *logfile_name = NULL;
	char *infile_name = "worktodo.ini";
	char *nfs_fbfile_name = NULL;
	uint32 flags;
	char manual_mode = 0;
	int i = 1;
	int32 deadline = 0;
	uint32 max_relations = 0;
	uint64 nfs_lower = 0;
	uint64 nfs_upper = 0;
	enum cpu_type cpu;
	uint32 cache_size1; 
	uint32 cache_size2; 
	uint32 num_threads = 0;
	uint32 mem_mb = 0;

	get_cache_sizes(&cache_size1, &cache_size2);
	cpu = get_cpu_type();

	flags = MSIEVE_FLAG_USE_LOGFILE;
	flags &= ~(MSIEVE_FLAG_USE_LOGFILE | MSIEVE_FLAG_LOG_TO_STDOUT);	//we don't need the logfile

	if (isdigit(number[0]) || number[0] == '(' )
		strncpy(buf, number, sizeof(buf));
	i++;

	get_random_seeds(&seed1, &seed2);

	if (isdigit(buf[0]) || buf[0] == '(' ) {
		return factor_integer(buf, flags, savefile_name, 
				logfile_name, nfs_fbfile_name,
				&seed1, &seed2,
				max_relations, 
				nfs_lower, nfs_upper, cpu,
				cache_size1, cache_size2,
				num_threads, mem_mb);
	}
	else if (manual_mode) {
		while (1) {
			printf("\n\nnext number: ");
			fflush(stdout);
			buf[0] = 0;
			fgets(buf, (int)sizeof(buf), stdin);
			return factor_integer(buf, flags, savefile_name, 
					logfile_name, nfs_fbfile_name,
					&seed1, &seed2,
					max_relations, 
					nfs_lower, nfs_upper, cpu,
					cache_size1, cache_size2,
					num_threads, mem_mb);
			if (feof(stdin))
				break;
		}
	}
	else {
		FILE *infile = fopen(infile_name, "r");
		if (infile == NULL) {
			printf("cannot open input file '%s'\n", infile_name);
			return 0;
		}

		while (1) {
			buf[0] = 0;
			fgets(buf, (int)sizeof(buf), infile);
			return factor_integer(buf, flags, savefile_name, 
					logfile_name, nfs_fbfile_name,
					&seed1, &seed2,
					max_relations, 
					nfs_lower, nfs_upper, cpu,
					cache_size1, cache_size2,
					num_threads, mem_mb);
			if (feof(infile))
				break;
		}
		fclose(infile);
	}

	return 0;
}
