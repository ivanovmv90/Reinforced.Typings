//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.


import {A, B} from "test1";

@Injectable()
export class HomeController
{
	constructor (private http: Http) { } 
	public Index() : Observable<Response>
	{
		var params = {  };
		return this.http.post('/Home/Index', params);
		
	}
	public Test() : Observable<Response>
	{
		var params = {  };
		return this.http.post('/Home/Test', params);
		
	}
}